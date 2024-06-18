using System;

namespace Appegy.Storage
{
    public class StorageDisposedException : Exception
    {
        public StorageDisposedException(string storageFilePath)
            : base($"Storage already disposed and can't be used anymore. File path: {storageFilePath}")
        {
        }
    }
}