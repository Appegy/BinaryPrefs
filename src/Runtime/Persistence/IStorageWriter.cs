using System;

namespace Appegy.Storage
{
    internal interface IStorageWriter
    {
        Action SaveDeferredChanges { get; set; }

        bool TryDeferSave();

        void Write(StorageSnapshot snapshot, bool waitForDisk);

        void Flush();
    }
}
