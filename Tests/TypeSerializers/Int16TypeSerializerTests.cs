using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Int16TypeSerializerTests : BaseTypeSerializerTests<short, Int16Serializer>
    {
        private static short[] Inputs => new[]
        {
            short.MinValue, // -32768
            short.MaxValue, // 32767
            (short)0, // zero
            (short)1, // smallest positive short
            (short)-1, // smallest negative short
            (short)1024, // power of two
            (short)-1024, // negative power of two
            (short)12345, // random positive value
            (short)-12345 // random negative value
        };

        public Int16TypeSerializerTests(short value) : base(value)
        {
        }
    }
}