using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace Appegy.Storage
{
    /// <summary> Manages a binary storage system for saving, retrieving, and managing records of various types. </summary>
    public partial class BinaryStorage : IDisposable
    {
        private readonly string _storageFilePath;
        private readonly IReadOnlyList<BinarySection> _supportedTypes;
        private readonly Dictionary<string, Record> _data = new();
        private readonly Dictionary<IReactiveCollection, string> _collections = new();
        private int _changeScopeCounter;

        /// <summary> Gets or sets a value indicating whether data should be saved automatically. </summary>
        public bool AutoSave { get; set; }

        /// <summary> Gets or sets the behavior when a requested key is not found in the storage. </summary>
        public MissingKeyBehavior MissingKeyBehavior { get; set; } = MissingKeyBehavior.ReturnDefaultValueOnly;

        /// <summary> Gets or sets the behavior when the type of value associated with a key does not match the expected type. </summary>
        public TypeMismatchBehaviour TypeMismatchBehaviour { get; set; } = TypeMismatchBehaviour.OverrideValueAndType;

        /// <summary> Gets a value indicating whether there are unsaved changes. </summary>
        public bool IsDirty { get; private set; }

        /// <summary> Gets a value indicating whether the storage has been disposed. </summary>
        public bool IsDisposed { get; private set; }

        /// <summary> Initializes a new instance of the <see cref="BinaryStorage"/> class. </summary>
        /// <param name="storageFilePath">The file path for storing data.</param>
        /// <param name="supportedTypes">The list of supported types for storage.</param>
        internal BinaryStorage(string storageFilePath, IReadOnlyList<BinarySection> supportedTypes)
        {
            _storageFilePath = storageFilePath;
            _supportedTypes = supportedTypes;
        }

        #region Events

        /// <summary> Occurs when a key is added to the storage. </summary>
        public event Action<string> OnKeyAdded;

        /// <summary> Occurs when a key is changed in the storage. </summary>
        public event Action<string> OnKeyChanged;

        /// <summary> Occurs when a key is removed from the storage. </summary>
        public event Action<string> OnKeyRemoved;

        #endregion

        #region Public API

        /// <summary> Determines whether the specified key exists in the storage. </summary>
        /// <param name="key">The key to check for existence.</param>
        /// <returns>True if the key exists; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        public virtual bool Has(string key)
        {
            ThrowIfDisposed();
            return _data.ContainsKey(key);
        }

        /// <summary> Gets the type of the value associated with the specified key. </summary>
        /// <param name="key">The key to get the type for.</param>
        /// <returns>The type of the value associated with the key, or null if the key does not exist.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        [CanBeNull]
        public virtual Type TypeOf(string key)
        {
            ThrowIfDisposed();
            return _data.TryGetValue(key, out var record) ? record.Type : null;
        }

        /// <summary> Determines whether the storage supports the specified type. </summary>
        /// <typeparam name="T">The type to check for support.</typeparam>
        /// <returns>True if the type is supported; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        public virtual bool Supports<T>()
        {
            ThrowIfDisposed();
            ThrowIfCollection<T>();
            return _supportedTypes.Any(c => c is TypedBinarySection<T>);
        }

        /// <summary> Gets the value associated with the specified key. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to get the value for.</param>
        /// <param name="defaultValue">The default value to use if the key does not exist.</param>
        /// <param name="overrideMissingKeyBehavior">Override default behavior when a requested key is not found in the storage.</param>
        /// <returns>The value associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        /// <exception cref="UnexpectedTypeException">Thrown if the type of the value associated with the key does not match the expected type.</exception>
        public virtual T Get<T>(string key, T defaultValue = default, MissingKeyBehavior? overrideMissingKeyBehavior = null)
        {
            ThrowIfDisposed();
            ThrowIfCollection<T>();
            var record = GetRecord(key);
            var missingKeyBehavior = overrideMissingKeyBehavior ?? MissingKeyBehavior;
            return record switch
            {
                Record<T> typedRecord => typedRecord.Value,
                not null => throw new UnexpectedTypeException(key, nameof(Get), record.Type, typeof(T)),
                null => missingKeyBehavior switch
                {
                    MissingKeyBehavior.InitializeWithDefaultValue => AddRecord(key, defaultValue).Value,
                    MissingKeyBehavior.ReturnDefaultValueOnly => defaultValue,
                    _ => throw new UnexpectedEnumException(typeof(MissingKeyBehavior), missingKeyBehavior)
                }
            };
        }

        /// <summary> Sets the value for the specified key. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to set the value for.</param>
        /// <param name="value">The value to set.</param>
        /// <param name="overrideTypeMismatchBehaviour">Whether to override the value if the key already exists but with another type.</param>
        /// <returns>True if the value was set; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        /// <exception cref="UnexpectedTypeException">Thrown if the type of the value associated with the key does not match the expected type.</exception>
        public virtual bool Set<T>(string key, T value, TypeMismatchBehaviour? overrideTypeMismatchBehaviour = null)
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
                return ChangeRecord(key, typedRecord, value);
            }

            var mismatchBehaviour = overrideTypeMismatchBehaviour ?? TypeMismatchBehaviour;
            switch (mismatchBehaviour)
            {
                case TypeMismatchBehaviour.OverrideValueAndType:
                    using (MultipleChangeScope())
                    {
                        RemoveRecord(key);
                        AddRecord(key, value);
                    }
                    return true;
                case TypeMismatchBehaviour.ThrowException:
                    throw new UnexpectedTypeException(key, nameof(Set), record.Type, typeof(T));
                case TypeMismatchBehaviour.Ignore:
                    return false;
                default:
                    throw new UnexpectedEnumException(typeof(TypeMismatchBehaviour), mismatchBehaviour);
            }
        }

        /// <summary>
        /// Removes the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key to remove the value for.</param>
        /// <returns>True if the key was removed; otherwise, false.</returns>
        public virtual bool Remove(string key)
        {
            ThrowIfDisposed();
            return RemoveRecord(key);
        }

        /// <summary>
        /// Removes values based on a predicate.
        /// </summary>
        /// <param name="predicate">The predicate to determine which keys to remove.</param>
        /// <returns>The number of keys removed.</returns>
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

        /// <summary>
        /// Removes all values from the storage.
        /// </summary>
        /// <returns>The number of keys removed.</returns>
        public virtual int RemoveAll()
        {
            ThrowIfDisposed();
            var count = _data.Count;
            RemoveAllRecords();
            return count;
        }

        /// <summary> Saves the current data to disk. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        public virtual void Save()
        {
            SaveDataFromDisk();
        }

        /// <summary> Begins a scope for making multiple changes. </summary>
        /// <returns>An IDisposable to end the scope.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        public IDisposable MultipleChangeScope()
        {
            ThrowIfDisposed();
            _changeScopeCounter++;
            return new DisposableScope(DecreaseCounter);
        }

        #region Collections

        /// <summary> Determines whether the storage supports lists of the specified type. </summary>
        /// <typeparam name="T">The type to check for support.</typeparam>
        /// <returns>True if lists of the type are supported; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        public virtual bool SupportsListsOf<T>() => SupportsCollectionOf<T, ReactiveList<T>>();

        /// <summary> Determines whether the storage supports sets of the specified type. </summary>
        /// <typeparam name="T">The type to check for support.</typeparam>
        /// <returns>True if sets of the type are supported; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        public virtual bool SupportsSetsOf<T>() => SupportsCollectionOf<T, ReactiveSet<T>>();

        /// <summary> Determines whether the storage supports dictionaries of the specified key and value types. </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <returns>True if dictionaries of the key and value types are supported; otherwise, false.</returns>
        public virtual bool SupportsDictionariesOf<TKey, TValue>() => SupportsCollectionOf<KeyValuePair<TKey, TValue>, ReactiveDictionary<TKey, TValue>>();

        /// <summary> Gets the list associated with the specified key. </summary>
        /// <typeparam name="T">The type of the list elements.</typeparam>
        /// <param name="key">The key to get the list for.</param>
        /// <returns>The list associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        public IList<T> GetListOf<T>(string key) => GetCollectionOf<T, ReactiveList<T>>(key);

        /// <summary> Gets the set associated with the specified key. </summary>
        /// <typeparam name="T">The type of the set elements.</typeparam>
        /// <param name="key">The key to get the set for.</param>
        /// <returns>The set associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        public ISet<T> GetSetOf<T>(string key) => GetCollectionOf<T, ReactiveSet<T>>(key);

        /// <summary> Gets the dictionary associated with the specified key. </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <param name="key">The key to get the dictionary for.</param>
        /// <returns>The dictionary associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        public IDictionary<TKey, TValue> GetDictionaryOf<TKey, TValue>(string key) => GetCollectionOf<KeyValuePair<TKey, TValue>, ReactiveDictionary<TKey, TValue>>(key);

        /// <summary> Determines whether the specified collection type is supported. </summary>
        /// <typeparam name="T">The type of elements in the collection.</typeparam>
        /// <typeparam name="TCollection">The type of the collection.</typeparam>
        /// <returns> <c>true</c> if the specified collection type is supported; otherwise, <c>false</c>. </returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private bool SupportsCollectionOf<T, TCollection>() where TCollection : IReactiveCollection
        {
            ThrowIfDisposed();
            return _supportedTypes.Any(c => c is TypedBinarySection<TCollection>);
        }

        /// <summary> Gets the collection associated with the specified key. </summary>
        /// <typeparam name="T">The type of the collection elements.</typeparam>
        /// <typeparam name="TCollection">The type of the collection.</typeparam>
        /// <param name="key">The key to get the collection for.</param>
        /// <returns>The collection associated with the key.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
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

        #endregion

        #region Mutable methods

        /// <summary> Adds a new record with the specified key and value. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to add the record for.</param>
        /// <param name="value">The value to add.</param>
        /// <returns>The added record.</returns>
        /// <exception cref="UnregisteredTypeException">Thrown if the type is not registered.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private Record<T> AddRecord<T>(string key, T value)
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
                _collections.Add(rc, key);
                rc.OnChanged += ReactiveCollectionChanged;
            }
            MarkChanged();
            OnKeyAdded?.Invoke(key);
            return record;
        }

        /// <summary> Changes the value of an existing record. </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="key">The key to change the record for.</param>
        /// <param name="record">The record to change.</param>
        /// <param name="value">The new value.</param>
        /// <returns>True if the value was changed; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private bool ChangeRecord<T>(string key, Record<T> record, T value)
        {
            var serializer = ((TypedBinarySection<T>)_supportedTypes[record.TypeIndex]).Serializer;
            var equals = serializer.Equals(record.Value, value);
            if (!equals)
            {
                record.Value = value;
                MarkChanged();
                OnKeyChanged?.Invoke(key);
            }
            return !equals;
        }

        /// <summary> Removes the record associated with the specified key. </summary>
        /// <param name="key">The key to remove the record for.</param>
        /// <returns>True if the record was removed; otherwise, false.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private bool RemoveRecord(string key)
        {
            if (!_data.TryGetValue(key, out var value))
            {
                return false;
            }
            if (value.Object is IReactiveCollection rc)
            {
                rc.OnChanged -= ReactiveCollectionChanged;
                rc.Dispose();
                _collections.Remove(rc);
            }
            _supportedTypes[value.TypeIndex].Count--;
            _data.Remove(key);
            MarkChanged();
            OnKeyRemoved?.Invoke(key);
            return true;
        }

        /// <summary> Removes all records from the storage. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private void RemoveAllRecords()
        {
            using (MultipleChangeScope())
            {
                foreach (var rc in _data.Values.Select(c => c.Object).OfType<IReactiveCollection>())
                {
                    rc.OnChanged -= ReactiveCollectionChanged;
                    rc.Dispose();
                    _collections.Remove(rc);
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

        #region Private methods

        /// <summary> Gets the record associated with the specified key. </summary>
        /// <param name="key">The key to get the record for.</param>
        /// <returns>The record associated with the key, or null if the key does not exist.</returns>
        [CanBeNull]
        private Record GetRecord(string key)
        {
            return _data.GetValueOrDefault(key);
        }

        /// <summary>
        /// Decreases the change scope counter and saves data if necessary.
        /// </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private void DecreaseCounter()
        {
            if (_changeScopeCounter == 0)
            {
                Debug.LogError($"{nameof(BinaryStorage)}: Unexpected behaviour - MultipleChangeScope counter is already zero");
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

        /// <summary> Reacts to a change in a reactive collection. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private void ReactiveCollectionChanged(IReactiveCollection collection)
        {
            if (_collections.TryGetValue(collection, out var key))
            {
                MarkChanged();
                OnKeyChanged?.Invoke(key);
            }
        }

        /// <summary> Marks the storage as changed and saves data if necessary. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private void MarkChanged()
        {
            if (!AutoSave || _changeScopeCounter > 0)
            {
                IsDirty = true;
                return;
            }
            SaveDataFromDisk();
        }

        /// <summary> Throws an exception if the storage has been disposed. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException($"{nameof(BinaryStorage)}: {_storageFilePath}");
            }
        }

        /// <summary> Throws an exception if the specified type is a collection. </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <exception cref="IncorrectUsageOfCollectionException">Thrown if the type is a collection.</exception>
        private void ThrowIfCollection<T>()
        {
            var type = typeof(T);
            if (type.IsCollection())
            {
                throw new IncorrectUsageOfCollectionException(nameof(Get), type);
            }
        }

        #endregion

        #region Dispose Pattern

        /// <summary> Finalizer </summary>
        ~BinaryStorage()
        {
            Dispose(false);
        }

        /// <summary> Disposes the resources used by the storage. </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary> Disposes the resources used by the storage. </summary>
        /// <param name="disposing">Whether managed resources should be disposed.</param>
        private void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            // Always dispose IReactiveCollection instances
            foreach (var rc in _data.Values.Select(c => c.Object).OfType<IReactiveCollection>())
            {
                rc.OnChanged -= ReactiveCollectionChanged;
                rc.Dispose();
                _collections.Remove(rc);
            }

            OnKeyAdded = null;
            OnKeyChanged = null;
            OnKeyRemoved = null;

            if (disposing)
            {
                _data.Clear();
                _supportedTypes.ForEach(static c => c.Count = 0);
            }

            IsDisposed = true;
            UnlockFilePathInEditor(_storageFilePath);
        }

        #endregion

        #region File System IO

        /// <summary> Loads the data from disk into memory. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        private void LoadDataFromDisk()
        {
            ThrowIfDisposed();
            BinaryStorageIO.LoadDataFromDisk(_storageFilePath, _supportedTypes, _data);
            foreach (var pair in _data)
            {
                if (pair.Value.Object is IReactiveCollection rc)
                {
                    _collections.Add(rc, pair.Key);
                    rc.OnChanged += ReactiveCollectionChanged;
                }
            }
        }

        /// <summary> Saves the data from memory to disk. </summary>
        /// <exception cref="ObjectDisposedException">Thrown if the storage is disposed.</exception>
        /// <exception cref="IOException"> An I/O error occurred </exception>
        private void SaveDataFromDisk()
        {
            ThrowIfDisposed();
            BinaryStorageIO.SaveDataOnDisk(_storageFilePath, _supportedTypes, _data);
            IsDirty = false;
        }

        #endregion
    }
}