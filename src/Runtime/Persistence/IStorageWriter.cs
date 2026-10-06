namespace Appegy.Storage
{
    internal interface IStorageWriter
    {
        void Write(StorageSnapshot snapshot, bool waitForDisk);

        bool Flush();
    }
}
