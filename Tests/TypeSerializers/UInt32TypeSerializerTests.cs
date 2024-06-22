using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class UInt32TypeSerializerTests : BaseTypeSerializerTests<uint, UInt32Serializer>
    {
        private static uint[] Inputs => new[]
        {
            uint.MinValue, // 0
            uint.MaxValue, // 4294967295
            0u, // zero
            1u, // smallest positive uint
            1024u, // power of two
            1234567890u // random positive value
        };

        public UInt32TypeSerializerTests(uint value) : base(value)
        {
        }
    }
}