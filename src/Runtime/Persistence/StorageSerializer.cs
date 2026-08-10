using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Appegy.Storage
{
    /// <summary> Turns the records of one storage into a <see cref="StorageSnapshot"/>, reusing a single pooled buffer between saves. </summary>
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

        public StorageSnapshot Serialize(Dictionary<string, Record> data, long generation)
        {
            if (data.Count == 0)
            {
                return StorageSnapshot.Empty(generation);
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
            return StorageSnapshot.Take(_stream, generation);
        }
    }
}
