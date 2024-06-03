using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Appegy.BinaryStorage
{
    public partial class BinaryPrefs : IDisposable
    {
        private readonly string _storageFilePath;
        private readonly bool _autoSave;
        private readonly IReadOnlyList<TypedBinarySection> _supportedTypes;
        private readonly Dictionary<string, Record> _data = new();

        public bool AutoSave { get; set; }
        public bool IsDirty { get; private set; }
        public bool IsDisposed { get; private set; }

        internal BinaryPrefs(string storageFilePath, IReadOnlyList<TypedBinarySection> supportedTypes)
        {
            _storageFilePath = storageFilePath;
            _supportedTypes = supportedTypes;
        }

        #region Public API

        public virtual bool Has(string key)
        {
            ThrowIfDisposed();
            return _data.ContainsKey(key);
        }

        [CanBeNull]
        public virtual Type TypeOf(string key)
        {
            ThrowIfDisposed();
            return _data.TryGetValue(key, out var record) ? record.Type : null;
        }

        public virtual bool Supports<T>()
        {
            ThrowIfDisposed();
            return _supportedTypes.Any(c => c is TypedBinarySection<T>);
        }

        public virtual T Get<T>(string key, T initValue = default)
        {
            ThrowIfDisposed();
            var record = GetRecord(key) ?? AddRecord(key, initValue);
            if (record is not Record<T> typedRecord)
            {
                throw new UnexpectedTypeException(key, "get", record.Type, typeof(T));
            }
            return typedRecord.Value;
        }

        public virtual bool Set<T>(string key, T value, bool overrideTypeIfExists = false)
        {
            ThrowIfDisposed();
            var record = GetRecord(key);
            if (record == null)
            {
                AddRecord(key, value);
                return true;
            }

            if (record is Record<T> typedRecord)
            {
                return ChangeRecord(typedRecord, value);
            }

            if (!overrideTypeIfExists)
            {
                throw new UnexpectedTypeException(key, "set", record.Type, typeof(T));
            }

            // TODO multiple change scope
            RemoveRecord(key);
            AddRecord(key, value);
            return true;
        }

        public virtual bool Remove(string key)
        {
            ThrowIfDisposed();
            return RemoveRecord(key);
        }

        public virtual int Remove(Func<string, bool> predicate)
        {
            ThrowIfDisposed();
            // TODO create buffer for deleting keys
            var keys = _data.Keys.Where(predicate).ToList();
            // TODO multiple change scope
            foreach (var key in keys)
            {
                RemoveRecord(key);
            }
            return keys.Count;
        }

        public virtual int RemoveAll()
        {
            ThrowIfDisposed();
            var count = _data.Count;
            RemoveAllRecords();
            return count;
        }

        public virtual void Save()
        {
            SaveDataFromDisk();
        }

        internal void SaveDataFromDisk()
        {
            ThrowIfDisposed();
            BinaryPrefsIO.SaveDataOnDisk(_storageFilePath, _supportedTypes, _data);
            IsDirty = false;
        }

        private void LoadDataFromDisk()
        {
            ThrowIfDisposed();
            BinaryPrefsIO.LoadDataFromDisk(_storageFilePath, _supportedTypes, _data);
        }

        public virtual void Dispose()
        {
            if (IsDisposed) return;
            _data.Clear();
            _supportedTypes.ForEach(static c => c.Count = 0);
            IsDisposed = true;
            UnlockFilePathInEditor(_storageFilePath);
        }

        #endregion

        #region Immutable methods

        [CanBeNull]
        private Record GetRecord(string key)
        {
            return _data.GetValueOrDefault(key);
        }

        #endregion

        #region Mutable methods

        private Record AddRecord<T>(string key, T value)
        {
            var typeIndex = _supportedTypes.FindIndex(static c => c is TypedBinarySection<T>);
            if (typeIndex == -1)
            {
                throw new UnregisteredTypeException(typeof(T));
            }

            var section = (TypedBinarySection<T>)_supportedTypes[typeIndex];
            var record = new Record<T>(value, typeIndex);
            section.Count++;
            _data.Add(key, record);
            MarkChanged();
            return record;
        }

        private bool ChangeRecord<T>(Record<T> record, T value)
        {
            var serializer = ((TypedBinarySection<T>)_supportedTypes[record.TypeIndex]).Serializer;
            var result = serializer.Equals(record.Value, value);
            if (!result)
            {
                record.Value = value;
                MarkChanged();
            }
            return !result;
        }

        private bool RemoveRecord(string key)
        {
            if (!_data.TryGetValue(key, out var value))
            {
                return false;
            }
            _supportedTypes[value.TypeIndex].Count--;
            _data.Remove(key);
            MarkChanged();
            return true;
        }

        private void RemoveAllRecords()
        {
            _data.Clear();
            foreach (var section in _supportedTypes)
            {
                section.Count = 0;
            }
            MarkChanged();
        }

        #endregion

        private void MarkChanged()
        {
            if (!AutoSave)
            {
                IsDirty = true;
                return;
            }
            SaveDataFromDisk();
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new StorageDisposedException(_storageFilePath);
            }
        }
    }
}