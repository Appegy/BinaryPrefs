using System.Buffers;

namespace Appegy.Storage
{
    /// <summary>
    /// A serialized storage state on its way to disk. Owns the pooled array holding the bytes: whoever receives a snapshot
    /// publishes it and then calls <see cref="Release"/> exactly once. An empty snapshot carries no bytes because the storage
    /// holds no records, and publishing it removes the file instead of writing it.
    /// </summary>
    internal readonly struct StorageSnapshot
    {
        public readonly byte[] Buffer;
        public readonly int Length;

        public StorageSnapshot(byte[] buffer, int length)
        {
            Buffer = buffer;
            Length = length;
        }

        public static StorageSnapshot Empty => default;

        public bool IsEmpty => Buffer == null;

        public void Release()
        {
            if (Buffer != null)
            {
                ArrayPool<byte>.Shared.Return(Buffer);
            }
        }
    }
}
