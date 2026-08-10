namespace Appegy.Storage
{
    /// <summary> Publishes on the calling thread, so every change is on disk before it returns and an I/O failure always reaches the caller. </summary>
    internal sealed class ImmediateStorageWriter : IStorageWriter
    {
        private readonly StorageFile _file;

        public ImmediateStorageWriter(StorageFile file)
        {
            _file = file;
        }

        public void Write(StorageSnapshot snapshot, bool waitForDisk)
        {
            _file.Publish(snapshot);
        }

        public void Flush()
        {
        }
    }
}
