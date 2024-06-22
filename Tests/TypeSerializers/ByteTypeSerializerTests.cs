using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class ByteTypeSerializerTests : BaseTypeSerializerTests<byte, ByteSerializer>
    {
        private static byte[] Inputs => new[]
        {
            byte.MinValue, // 0
            byte.MaxValue, // 255
            (byte)1, // Smallest positive byte
            (byte)127, // Middle value
            (byte)64, // Power of two
            (byte)3, // Just below a power of two
            (byte)10, // Random small value
            (byte)100, // Random middle value
            (byte)200, // Random large value
        };

        public ByteTypeSerializerTests(byte value) : base(value)
        {
        }
    }
}