using System;
using System.Collections.Concurrent;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace Appegy.Storage
{
    /// <summary>
    /// Publishes on a background thread shared by every storage, so a change does not block the caller on the disk. Holds a
    /// single slot: a newer snapshot replaces an unwritten older one, because the file is always written whole and only the
    /// last state matters. A write that has to wait for the disk is published on the calling thread instead of being queued,
    /// so it never lines up behind another storage.
    /// </summary>
    internal sealed class BackgroundStorageWriter : IStorageWriter
    {
        private const string ThreadName = "BinaryPrefs.Writer";

        private static readonly BlockingCollection<BackgroundStorageWriter> _scheduled = new();
        private static readonly object _threadLock = new();
        private static Thread _thread;

        private readonly StorageFile _file;
        private readonly object _lock = new();

        private StorageSnapshot? _pending;
        private bool _isScheduled;
        private bool _isPublishing;

        public BackgroundStorageWriter(StorageFile file)
        {
            _file = file;
        }

        public void Write(StorageSnapshot snapshot, bool waitForDisk)
        {
            if (!waitForDisk)
            {
                Schedule(snapshot);
                return;
            }
            TakePending()?.Release();
            _file.Publish(snapshot);
        }

        public void Flush()
        {
            var pending = TakePending();
            if (pending != null)
            {
                _file.Publish(pending.Value);
            }
        }

        private void Schedule(StorageSnapshot snapshot)
        {
            StorageSnapshot? replaced;
            lock (_lock)
            {
                replaced = _pending;
                _pending = snapshot;
                if (!_isScheduled)
                {
                    _isScheduled = true;
                    EnsureThreadStarted();
                    _scheduled.Add(this);
                }
            }
            replaced?.Release();
        }

        /// <summary> Take the slot over, after waiting out a publish that is already under way, so the caller can publish a newer state itself. </summary>
        private StorageSnapshot? TakePending()
        {
            lock (_lock)
            {
                while (_isPublishing)
                {
                    Monitor.Wait(_lock);
                }
                var pending = _pending;
                _pending = null;
                return pending;
            }
        }

        private void PublishScheduled()
        {
            StorageSnapshot snapshot;
            lock (_lock)
            {
                _isScheduled = false;
                if (_pending == null)
                {
                    return;
                }
                snapshot = _pending.Value;
                _pending = null;
                _isPublishing = true;
            }

            try
            {
                _file.Publish(snapshot);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save storage '{_file.Main}'. Reason: {exception.Message}");
            }
            finally
            {
                lock (_lock)
                {
                    _isPublishing = false;
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
            lock (_threadLock)
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
            foreach (var writer in _scheduled.GetConsumingEnumerable())
            {
                writer.PublishScheduled();
            }
        }
    }
}
