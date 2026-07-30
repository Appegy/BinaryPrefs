using System;

namespace Appegy.Storage
{
    public partial class BinaryStorage
    {
        private readonly struct ChangeScope : IDisposable
        {
            private readonly BinaryStorage _storage;

            public ChangeScope(BinaryStorage storage)
            {
                _storage = storage;
                _storage._changeScopeCounter++;
            }

            public void Dispose()
            {
                _storage.DecreaseCounter();
            }
        }
    }
}
