namespace Appegy.Storage
{
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
