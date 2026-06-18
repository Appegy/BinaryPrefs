using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Int64TypeSerializerTests : BaseTypeSerializerTests<long, Int64Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { long.MinValue, "min" },
            new object[] { long.MaxValue, "max" },
            new object[] { 0L, "zero" },
            new object[] { 1L, "one" },
            new object[] { -1L, "minus_one" },
            new object[] { 1024L, "pow2" },
            new object[] { -1024L, "minus_pow2" },
            new object[] { 1234567890123L, "rnd_pos" }
        };

        public Int64TypeSerializerTests(long value, string _) : base(value)
        {
        }
    }
}