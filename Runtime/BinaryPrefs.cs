using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace Appegy.BinaryStorage
{
    public partial class BinaryPrefs : IDisposable
    {
        private readonly string _storageFilePath;
        private readonly bool _autoSave;
        private readonly IReadOnlyList<BinarySection> _supportedTypes;
        private readonly Dictionary<string, Record> _data = new();
        private int _changeScopeCounter;

        public bool AutoSave { get; set; }
        public bool IsDirty { get; private set; }
        public bool IsDisposed { get; private set; }

        internal BinaryPrefs(string storageFilePath, IReadOnlyList<BinarySection> supportedTypes)
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
            ThrowIfCollection<T>();
            return _supportedTypes.Any(c => c is TypedBinarySection<T>);
        }

        public virtual T Get<T>(string key, T initValue = default)
        {
            ThrowIfDisposed();
            ThrowIfCollection<T>();
            var record = GetRecord(key) ?? AddRecord(key, initValue);
            if (record is not Record<T> typedRecord)
            {
                throw new UnexpectedTypeException(key, nameof(Get), record.Type, typeof(T));
            }
            return typedRecord.Value;
        }

        public virtual bool Set<T>(string key, T value, bool overrideTypeIfExists = false)
        {
            ThrowIfDisposed();
            ThrowIfCollection<T>();

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
                throw new UnexpectedTypeException(key, nameof(Set), record.Type, typeof(T));
            }

            using (MultipleChangeScope())
            {
                RemoveRecord(key);
                AddRecord(key, value);
            }

            return true;
        }

        #region Collections

        public virtual bool SupportsListsOf<T>()
        {
            ThrowIfDisposed();
            return _supportedTypes.Any(c => c is TypedBinarySection<ReactiveList<T>>);
        }

        public virtual bool SupportsSetsOf<T>()
        {
            ThrowIfDisposed();
            return _supportedTypes.Any(c => c is TypedBinarySection<ReactiveSet<T>>);
        }

        public virtual bool SupportsDictionariesOf<TKey, TValue>()
        {
            ThrowIfDisposed();
            return _supportedTypes.Any(c => c is TypedBinarySection<ReactiveDictionary<TKey, TValue>>);
        }

        public IList<T> GetListOf<T>(string key)
        {
            return GetCollectionOf<T, ReactiveList<T>>(key);
        }

        public ISet<T> GetSetOf<T>(string key)
        {
            return GetCollectionOf<T, ReactiveSet<T>>(key);
        }

        public IDictionary<TKey, TValue> GetDictionaryOf<TKey, TValue>(string key)
        {
            return GetCollectionOf<KeyValuePair<TKey, TValue>, ReactiveDictionary<TKey, TValue>>(key);
        }

        private TCollection GetCollectionOf<T, TCollection>(string key)
            where TCollection : ICollection<T>, IReactiveCollection, new()
        {
            ThrowIfDisposed();
            var record = GetRecord(key) ?? AddRecord(key, new TCollection());
            if (record is not Record<TCollection> typedRecord)
            {
                throw new UnexpectedTypeException(key, nameof(Get), record.Type, typeof(IList<T>));
            }
            return typedRecord.Value;
        }

        #endregion

        public virtual bool Remove(string key)
        {
            ThrowIfDisposed();
            return RemoveRecord(key);
        }

        public virtual int Remove(Func<string, bool> predicate)
        {
            ThrowIfDisposed();
            var keys = ListPool<string>.Get();
            keys.AddRange(_data.Keys.Where(predicate));
            using (MultipleChangeScope())
            {
                foreach (var key in keys)
                {
                    RemoveRecord(key);
                }
            }
            var removed = keys.Count;
            ListPool<string>.Release(keys);
            return removed;
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

        public IDisposable MultipleChangeScope()
        {
            ThrowIfDisposed();
            _changeScopeCounter++;
            return new DisposableScope(DecreaseCounter);
        }

        private void SaveDataFromDisk()
        {
            ThrowIfDisposed();
            BinaryPrefsIO.SaveDataOnDisk(_storageFilePath, _supportedTypes, _data);
            IsDirty = false;
        }

        private void LoadDataFromDisk()
        {
            ThrowIfDisposed();
            BinaryPrefsIO.LoadDataFromDisk(_storageFilePath, _supportedTypes, _data);
            foreach (var rc in _data.Values.Select(c => c.Object).OfType<IReactiveCollection>())
            {
                rc.OnChanged += MarkChanged;
            }
        }

        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
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
            if (value is IReactiveCollection rc)
            {
                rc.OnChanged += MarkChanged;
            }
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
            if (value.Object is IReactiveCollection rc)
            {
                rc.OnChanged -= MarkChanged;
                rc.Dispose();
            }
            _supportedTypes[value.TypeIndex].Count--;
            _data.Remove(key);
            MarkChanged();
            return true;
        }

        private void RemoveAllRecords()
        {
            using (MultipleChangeScope())
            {
                foreach (var rc in _data.Values.Select(c => c.Object).OfType<IReactiveCollection>())
                {
                    rc.OnChanged -= MarkChanged;
                    rc.Dispose();
                }
                _data.Clear();
                foreach (var section in _supportedTypes)
                {
                    section.Count = 0;
                }
                MarkChanged();
            }
        }

        #endregion

        private void DecreaseCounter()
        {
            if (_changeScopeCounter == 0)
            {
                Debug.LogError($"{nameof(BinaryPrefs)}: Unexpected behaviour - MultipleChangeScope counter is already zero");
                return;
            }
            _changeScopeCounter--;
            if (IsDisposed)
            {
                return;
            }
            if (_changeScopeCounter == 0 && IsDirty && AutoSave)
            {
                SaveDataFromDisk();
            }
        }

        private void MarkChanged()
        {
            if (!AutoSave || _changeScopeCounter > 0)
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

        private void ThrowIfCollection<T>()
        {
            var type = typeof(T);
            if (type.IsCollection())
            {
                throw new IncorrectUsageOfCollectionException(nameof(Get), type);
            }
        }
    }
}