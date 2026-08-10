using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Appegy.Storage
{
    internal sealed class StorageSerializer
    {
        private readonly IReadOnlyList<BinarySection> _sections;
        private readonly PooledMemoryStream _stream = new();
        private readonly BinaryWriter _writer;

        public StorageSerializer(IReadOnlyList<BinarySection> sections)
        {
            _sections = sections;
            _writer = new BinaryWriter(_stream, Encoding.UTF8);
        }

        internal int BufferCapacity => _stream.Capacity;

        public StorageSnapshot Serialize(Dictionary<string, Record> data)
        {
            if (data.Count == 0)
            {
                return StorageSnapshot.Empty;
            }

            _stream.Reset();
            try
            {
                StorageFormat.Write(_writer, _sections, data);
            }
            catch
            {
                _stream.Release();
                throw;
            }
            var buffer = _stream.Detach(out var length);
            return new StorageSnapshot(buffer, length);
        }

        public bool TryDeserialize(string filePath, Dictionary<string, Record> data, KeyLoadFailedBehaviour keyLoadFailedBehaviour, out StorageFileCorruptedException failure)
        {
            try
            {
                StorageFormat.ReadFile(filePath, _sections, data, keyLoadFailedBehaviour);
                failure = null;
                return true;
            }
            catch (StorageFileCorruptedException exception)
            {
                Clear(data);
                failure = exception;
                return false;
            }
        }

        public void Clear(Dictionary<string, Record> data)
        {
            data.Clear();
            for (var i = 0; i < _sections.Count; i++)
            {
                _sections[i].Count = 0;
            }
        }
    }
}
