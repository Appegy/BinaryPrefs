using System.Buffers;

namespace Appegy.Storage
{
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
