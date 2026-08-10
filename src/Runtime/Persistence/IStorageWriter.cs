namespace Appegy.Storage
{
    internal interface IStorageWriter
    {
        bool TryDeferSave();

        void Write(StorageSnapshot snapshot, bool waitForDisk);

        void Flush();
    }
}
