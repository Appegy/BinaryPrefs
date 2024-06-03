using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class UInt64TypeSerializerTests : BaseTypeSerializerTests<ulong, UInt64Serializer>
    {
        private static ulong[] Inputs => new[]
        {
            ulong.MinValue, // 0
            ulong.MaxValue, // 18446744073709551615
            0UL, // zero
            1UL, // smallest positive ulong
            1024UL, // power of two
            1234567890123UL // random positive value
        };

        public UInt64TypeSerializerTests(ulong value) : base(value)
        {
        }
    }
}