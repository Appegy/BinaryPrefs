using System;

namespace Appegy.Storage
{
    internal class DisposableScope : IDisposable
    {
        private readonly Action _disposeCallback;
        private bool _disposed;

        public DisposableScope(Action disposeCallback)
        {
            _disposeCallback = disposeCallback;
        }

        void IDisposable.Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _disposeCallback?.Invoke();
        }
    }
}