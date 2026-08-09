using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class UInt64TypeSerializerTests : BaseTypeSerializerTests<ulong, UInt64Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { ulong.MinValue, "min" },
            new object[] { ulong.MaxValue, "max" },
            new object[] { 0UL, "zero" },
            new object[] { 1UL, "one" },
            new object[] { 1024UL, "pow2" },
            new object[] { 1234567890123UL, "rnd_pos" }
        };

        public UInt64TypeSerializerTests(ulong value, string _) : base(value)
        {
        }
    }
}
