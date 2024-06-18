using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class UInt16TypeSerializerTests : BaseTypeSerializerTests<ushort, UInt16Serializer>
    {
        private static ushort[] Inputs => new[]
        {
            ushort.MinValue, // 0
            ushort.MaxValue, // 65535
            (ushort)0, // zero
            (ushort)1, // smallest positive ushort
            (ushort)1024, // power of two
            (ushort)12345 // random positive value
        };

        public UInt16TypeSerializerTests(ushort value) : base(value)
        {
        }
    }
}