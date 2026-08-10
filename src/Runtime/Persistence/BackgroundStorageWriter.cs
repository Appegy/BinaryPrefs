using System;
using System.Collections.Concurrent;
using System.Threading;
using Debug = UnityEngine.Debug;

namespace Appegy.Storage
{
    internal sealed class BackgroundStorageWriter : IStorageWriter
    {
        private const string ThreadName = "BinaryPrefs.Writer";

        private static readonly BlockingCollection<BackgroundStorageWriter> _scheduled = new();
        private static readonly object _threadLock = new();
        private static Thread _thread;

        private readonly StorageFile _file;
        private readonly object _lock = new();
        private readonly SynchronizationContext _context;
        private readonly SendOrPostCallback _saveDeferredChanges;

        private StorageSnapshot? _pending;
        private bool _isScheduled;
        private bool _isPublishing;
        private bool _isSaveDeferred;

        public Action SaveDeferredChanges { get; set; }

        public BackgroundStorageWriter(StorageFile file)
        {
            _file = file;
            _context = SynchronizationContext.Current;
            _saveDeferredChanges = _ => SaveDeferredChanges?.Invoke();
        }

        public bool TryDeferSave()
        {
            if (_context == null)
            {
                return false;
            }
            lock (_lock)
            {
                if (_pending == null && !_isPublishing)
                {
                    return false;
                }
                _isSaveDeferred = true;
                return true;
            }
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

        private StorageSnapshot? TakePending()
        {
            lock (_lock)
            {
                while (_isPublishing)
                {
                    Monitor.Wait(_lock);
                }
                _isSaveDeferred = false;
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

            var published = false;
            try
            {
                _file.Publish(snapshot);
                published = true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save storage '{_file.Main}'. Reason: {exception.Message}");
            }
            finally
            {
                bool saveDeferredChanges;
                lock (_lock)
                {
                    _isPublishing = false;
                    saveDeferredChanges = _isSaveDeferred && published;
                    _isSaveDeferred = false;
                    Monitor.PulseAll(_lock);
                }
                if (saveDeferredChanges)
                {
                    _context.Post(_saveDeferredChanges, null);
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
