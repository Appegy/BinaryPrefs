using System;

namespace Appegy.Storage
{
    internal abstract class ReactiveCollection : IReactiveCollection
    {
        public bool IsDisposed { get; private set; }

        public event Action<IReactiveCollection> OnChanged;

        protected abstract string ObjectName { get; }

        public abstract void Clear();

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            Clear();
            IsDisposed = true;
        }

        protected void SetDirty()
        {
            OnChanged?.Invoke(this);
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }
    }
}
