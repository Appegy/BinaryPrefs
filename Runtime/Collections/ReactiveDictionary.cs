using System;
using System.Collections;
using System.Collections.Generic;

namespace Appegy.Storage
{
    internal class ReactiveDictionary<TKey, TValue> : IReactiveCollection, IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new();

        public bool IsDisposed { get; private set; }

        public event Action<IReactiveCollection> OnChanged;

        private void SetDirty()
        {
            OnChanged?.Invoke(this);
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ReactiveDictionary<TKey, TValue>));
            }
        }

        #region Mutable functionallity

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            Clear();
            IsDisposed = true;
        }

        public TValue this[TKey key]
        {
            get
            {
                ThrowIfDisposed();
                return _dictionary[key];
            }
            set
            {
                ThrowIfDisposed();
                _dictionary[key] = value;
                SetDirty();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            ThrowIfDisposed();
            ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Add(item);
            SetDirty();
        }

        public void Clear()
        {
            ThrowIfDisposed();
            _dictionary.Clear();
            SetDirty();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            ThrowIfDisposed();
            var removed = ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Remove(item);
            if (removed)
            {
                SetDirty();
            }
            return removed;
        }

        public void Add(TKey key, TValue value)
        {
            ThrowIfDisposed();
            _dictionary.Add(key, value);
            SetDirty();
        }

        public bool Remove(TKey key)
        {
            ThrowIfDisposed();
            var removed = _dictionary.Remove(key);
            if (removed)
            {
                SetDirty();
            }
            return removed;
        }

        #endregion

        #region Immutable functionallity

        public int Count => _dictionary.Count;

        public bool IsReadOnly => ((IDictionary<TKey, TValue>)_dictionary).IsReadOnly;

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);

        #endregion
    }
}