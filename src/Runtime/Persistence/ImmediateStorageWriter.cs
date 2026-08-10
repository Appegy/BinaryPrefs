namespace Appegy.Storage
{
    internal sealed class ImmediateStorageWriter : IStorageWriter
    {
        private readonly StorageFile _file;

        public ImmediateStorageWriter(StorageFile file)
        {
            _file = file;
        }

        public bool TryDeferSave()
        {
            return false;
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
