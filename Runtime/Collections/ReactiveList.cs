using System;
using System.Collections;
using System.Collections.Generic;

namespace Appegy.BinaryStorage
{
    public class ReactiveList<T> : IReactiveCollection, IList<T>, IReadOnlyList<T>
    {
        private readonly List<T> _list = new();

        public string Key { get; internal set; }
        public bool IsDisposed { get; private set; }

        public event Action OnChanged;

        private void SetDirty()
        {
            OnChanged?.Invoke();
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new CollectionDisposedException(Key);
            }
        }

        #region Mutable functionallity

        public T this[int index]
        {
            get
            {
                ThrowIfDisposed();
                return _list[index];
            }
            set
            {
                ThrowIfDisposed();
                _list[index] = value;
                SetDirty();
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            Clear();
            IsDisposed = true;
        }

        public void Add(T item)
        {
            ThrowIfDisposed();
            _list.Add(item);
            SetDirty();
        }

        public void Clear()
        {
            ThrowIfDisposed();
            if (_list.Count > 0)
            {
                _list.Clear();
                SetDirty();
            }
        }

        public bool Remove(T item)
        {
            ThrowIfDisposed();
            var removed = _list.Remove(item);
            if (removed)
            {
                SetDirty();
            }
            return removed;
        }

        public void Insert(int index, T item)
        {
            ThrowIfDisposed();
            _list.Insert(index, item);
            SetDirty();
        }

        public void RemoveAt(int index)
        {
            ThrowIfDisposed();
            _list.RemoveAt(index);
            SetDirty();
        }

        #endregion

        #region Immutable functionallity

        public int Count => _list.Count;

        public bool IsReadOnly => IsDisposed || ((IList<T>)_list).IsReadOnly;

        public int IndexOf(T item) => _list.IndexOf(item);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        #endregion
    }
}