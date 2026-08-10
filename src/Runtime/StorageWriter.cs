using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace Appegy.Storage
{
    /// <summary>
    /// Publishes serialized storage bytes on a background thread. Holds a single slot per storage: a newer generation
    /// replaces an unwritten older one, because the file is always written whole and only the last state matters.
    /// </summary>
    internal sealed class StorageWriter
    {
        private const string ThreadName = "BinaryPrefs.Writer";

        private static readonly BlockingCollection<StorageWriter> Scheduled = new();
        private static readonly object ThreadLock = new();
        private static Thread _thread;

        private readonly StorageFilePaths _paths;
        private readonly object _lock = new();

        private byte[] _pendingBuffer;
        private int _pendingLength;
        private bool _pendingIsDelete;
        private bool _hasPending;
        private bool _isScheduled;
        private long _requestedGeneration;
        private long _publishedGeneration;

        public StorageWriter(in StorageFilePaths paths)
        {
            _paths = paths;
        }

        /// <summary> Hand serialized bytes over to the writer. The writer owns the array from now on. </summary>
        public void EnqueueWrite(byte[] buffer, int length)
        {
            Enqueue(buffer, length, false);
        }

        /// <summary> Ask the writer to remove the storage file and its companions. </summary>
        public void EnqueueDelete()
        {
            Enqueue(null, 0, true);
        }

        /// <summary> Block until everything enqueued so far has reached the disk. </summary>
        /// <param name="timeoutMilliseconds"> How long to wait, or <see cref="Timeout.Infinite"/> to wait as long as it takes </param>
        /// <returns> True when the disk caught up, false when the timeout expired first </returns>
        public bool Flush(int timeoutMilliseconds)
        {
            lock (_lock)
            {
                var target = _requestedGeneration;
                if (timeoutMilliseconds < 0)
                {
                    while (_publishedGeneration < target)
                    {
                        Monitor.Wait(_lock);
                    }
                    return true;
                }

                var elapsed = Stopwatch.StartNew();
                while (_publishedGeneration < target)
                {
                    var remaining = timeoutMilliseconds - (int)elapsed.ElapsedMilliseconds;
                    if (remaining <= 0)
                    {
                        return false;
                    }
                    Monitor.Wait(_lock, remaining);
                }
                return true;
            }
        }

        private void Enqueue(byte[] buffer, int length, bool isDelete)
        {
            byte[] replaced;
            lock (_lock)
            {
                replaced = _pendingBuffer;
                _pendingBuffer = buffer;
                _pendingLength = length;
                _pendingIsDelete = isDelete;
                _hasPending = true;
                _requestedGeneration++;

                if (!_isScheduled)
                {
                    _isScheduled = true;
                    EnsureThreadStarted();
                    Scheduled.Add(this);
                }
            }
            PooledMemoryStream.ReturnDetachedBuffer(replaced);
        }

        private void PublishPending()
        {
            byte[] buffer;
            int length;
            bool isDelete;
            long generation;

            lock (_lock)
            {
                _isScheduled = false;
                if (!_hasPending)
                {
                    return;
                }
                buffer = _pendingBuffer;
                length = _pendingLength;
                isDelete = _pendingIsDelete;
                generation = _requestedGeneration;
                _pendingBuffer = null;
                _hasPending = false;
            }

            try
            {
                if (isDelete)
                {
                    BinaryStorageIO.DeleteStorageFiles(_paths);
                }
                else
                {
                    BinaryStorageIO.WriteBufferOnDisk(_paths, buffer, length);
                }
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save storage '{_paths.Main}'. Reason: {exception.Message}");
            }
            finally
            {
                PooledMemoryStream.ReturnDetachedBuffer(buffer);
                lock (_lock)
                {
                    _publishedGeneration = generation;
                    Monitor.PulseAll(_lock);
                }
            }
        }

        private static void EnsureThreadStarted()
        {
            if (_thread != null)
            {
                return;
            }
            lock (ThreadLock)
            {
                if (_thread != null)
                {
                    return;
                }
                _thread = new Thread(WriteLoop)
                {
                    Name = ThreadName,
                    IsBackground = true
                };
                _thread.Start();
            }
        }

        private static void WriteLoop()
        {
            foreach (var writer in Scheduled.GetConsumingEnumerable())
            {
                writer.PublishPending();
            }
        }
    }
}
