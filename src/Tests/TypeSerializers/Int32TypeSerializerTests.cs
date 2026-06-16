using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Int32TypeSerializerTests : BaseTypeSerializerTests<int, Int32Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { int.MinValue, "min" },
            new object[] { int.MaxValue, "max" },
            new object[] { 0, "zero" },
            new object[] { 1, "one" },
            new object[] { -1, "minus_one" },
            new object[] { 1024, "pow2" },
            new object[] { -1024, "minus_pow2" },
            new object[] { 1234567890, "rnd_pos" }
        };

        public Int32TypeSerializerTests(int value, string _) : base(value)
        {
        }
    }
}