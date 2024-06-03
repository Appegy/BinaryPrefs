using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Appegy.BinaryStorage
{
    public partial class BinaryPrefs : IDisposable
    {
        private readonly string _storageFilePath;
        private readonly List<object> _serializers;
        private readonly bool _multiThreadSupport;
        private readonly IDictionary<string, ValueRecord> _data;

        public bool Disposed { get; private set; }

        private BinaryPrefs(string storageFilePath, List<object> serializers, bool multiThreadSupport)
        {
            _storageFilePath = storageFilePath;
            _serializers = serializers;
            _data = multiThreadSupport
                ? new ConcurrentDictionary<string, ValueRecord>()
                : new Dictionary<string, ValueRecord>();
        }

        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }

        [CanBeNull]
        public Type GetTypeFor(string key)
        {
            return _data.TryGetValue(key, out var record) ? record.Type : null;
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            var record = GetRecord(key) ?? AddRecord(key, defaultValue);
            if (record is not ValueRecord<T> typedRecord)
            {
                throw new UnexpectedTypeException(key, "get", record.Type, typeof(T));
            }
            return typedRecord.GetValue();
        }

        public bool Set<T>(string key, T value, bool overrideTypeIfExists = false)
        {
            var record = GetRecord(key);
            if (record == null)
            {
                AddRecord(key, value);
                return true;
            }

            if (record is ValueRecord<T> typedRecord)
            {
                return ChangeRecord(typedRecord, value);
            }

            if (!overrideTypeIfExists)
            {
                throw new UnexpectedTypeException(key, "set", record.Type, typeof(T));
            }

            RemoveRecord(key);
            AddRecord(key, value);
            return true;
        }

        public bool Remove(string key)
        {
            return RemoveRecord(key);
        }

        [CanBeNull]
        private ValueRecord GetRecord(string key)
        {
            // TODO: thread-safe get data
            return _data.TryGetValue(key, out var record) ? record : null;
        }

        private ValueRecord AddRecord<T>(string key, T value)
        {
            var serializer = GetSerializerFor<T>();
            if (serializer == null)
            {
                throw new UnregisteredTypeException(typeof(T));
            }
            var record = new ValueRecord<T>(serializer, value);
            // TODO: thread-safe add and save data
            _data.Add(key, record);
            return record;
        }

        private bool ChangeRecord<T>(ValueRecord<T> record, T value)
        {
            // TODO: thread-safe change and save data
            return record.SetValue(value);
        }

        private bool RemoveRecord(string key)
        {
            // TODO: thread-safe remove and save data
            return _data.Remove(key);
        }

        private TypeSerializer<T> GetSerializerFor<T>()
        {
            var serializer = _serializers.SingleOrDefault(c => c is TypeSerializer<T>) as TypeSerializer<T>;
            return serializer;
        }

        internal void SaveDataOnDisk()
        {
            ThrowIfDisposed();
        }

        internal void LoadDataFromDisk()
        {
            ThrowIfDisposed();
        }

        private void ThrowIfDisposed()
        {
            if (Disposed)
            {
                throw new StorageDisposedException(_storageFilePath);
            }
        }

        public void Dispose()
        {
            if (Disposed) return;
            _serializers.Clear();
            _data.Clear();
            Disposed = true;
            UnlockFilePathInEditor(_storageFilePath);
        }
    }
}