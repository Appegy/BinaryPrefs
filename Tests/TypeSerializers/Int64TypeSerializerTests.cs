using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Int64TypeSerializerTests : BaseTypeSerializerTests<long, Int64Serializer>
    {
        private static long[] Inputs => new[]
        {
            long.MinValue,  // -9223372036854775808
            long.MaxValue,  // 9223372036854775807
            0L,             // zero
            1L,             // smallest positive long
            -1L,            // smallest negative long
            1024L,          // power of two
            -1024L,         // negative power of two
            1234567890123L  // random positive value
        };

        public Int64TypeSerializerTests(long value) : base(value)
        {
        }
    }
}