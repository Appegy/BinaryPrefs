namespace Appegy.Storage
{
    internal interface IStorageWriter
    {
        void Write(StorageSnapshot snapshot, bool waitForDisk);

        void Flush();
    }
}
