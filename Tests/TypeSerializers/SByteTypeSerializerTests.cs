using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class SByteTypeSerializerTests : BaseTypeSerializerTests<sbyte, SByteSerializer>
    {
        private static sbyte[] Inputs => new[]
        {
            sbyte.MinValue, // -128
            sbyte.MaxValue, // 127
            (sbyte)0, // zero
            (sbyte)1, // smallest positive sbyte
            (sbyte)-1, // smallest negative sbyte
            (sbyte)64, // positive power of two
            (sbyte)-64, // negative power of two
            (sbyte)10, // random positive value
            (sbyte)-10 // random negative value
        };

        public SByteTypeSerializerTests(sbyte value) : base(value)
        {
        }
    }
}