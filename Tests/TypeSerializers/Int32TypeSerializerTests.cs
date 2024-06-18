using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Int32TypeSerializerTests : BaseTypeSerializerTests<int, Int32Serializer>
    {
        private static int[] Inputs => new[]
        {
            int.MinValue, // -2147483648
            int.MaxValue, // 2147483647
            0, // zero
            1, // smallest positive int
            -1, // smallest negative int
            1024, // power of two
            -1024, // negative power of two
            1234567890 // random positive value
        };

        public Int32TypeSerializerTests(int value) : base(value)
        {
        }
    }
}