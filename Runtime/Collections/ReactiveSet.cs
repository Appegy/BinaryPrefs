using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Appegy.BinaryStorage
{
    public class ReactiveSet<T> : IReactiveCollection, ISet<T>, IReadOnlyCollection<T>
    {
        private readonly HashSet<T> _set = new();

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
                throw new CollectionDisposedException();
            }
        }

        #region Mutable functionallity

        public void Dispose()
        {
            ThrowIfDisposed();
            if (IsDisposed)
            {
                return;
            }
            Clear();
            IsDisposed = true;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            ThrowIfDisposed();
            var count = Count;
            _set.ExceptWith(other);
            if (Count != count)
            {
                SetDirty();
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            ThrowIfDisposed();
            var count = Count;
            _set.IntersectWith(other);
            if (Count != count)
            {
                SetDirty();
            }
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            ThrowIfDisposed();
            var count = Count;
            _set.SymmetricExceptWith(other);
            if (Count != count)
            {
                SetDirty();
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            ThrowIfDisposed();
            var count = Count;
            _set.UnionWith(other);
            if (Count != count)
            {
                SetDirty();
            }
        }

        void ICollection<T>.Add(T item) => Add(item);

        public bool Add([CanBeNull] T item)
        {
            ThrowIfDisposed();
            var added = _set.Add(item);
            if (added)
            {
                SetDirty();
            }
            return added;
        }

        public void Clear()
        {
            ThrowIfDisposed();
            var count = Count;
            _set.Clear();
            if (Count != count)
            {
                SetDirty();
            }
        }

        public bool Remove(T item)
        {
            ThrowIfDisposed();
            var removed = _set.Remove(item);
            if (removed)
            {
                SetDirty();
            }
            return removed;
        }

        #endregion

        #region Immutable functionallity

        public int Count => _set.Count;

        public bool IsReadOnly => IsDisposed || ((ISet<T>)_set).IsReadOnly;

        public IEnumerator<T> GetEnumerator() => _set.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool IsProperSubsetOf(IEnumerable<T> other) => _set.IsProperSubsetOf(other);

        public bool IsProperSupersetOf(IEnumerable<T> other) => _set.IsProperSupersetOf(other);

        public bool IsSubsetOf(IEnumerable<T> other) => _set.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<T> other) => _set.IsSupersetOf(other);

        public bool Overlaps(IEnumerable<T> other) => _set.Overlaps(other);

        public bool SetEquals(IEnumerable<T> other) => _set.SetEquals(other);

        public bool Contains(T item) => _set.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _set.CopyTo(array, arrayIndex);

        #endregion
    }
}