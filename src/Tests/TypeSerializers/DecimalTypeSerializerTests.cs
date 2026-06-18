using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class DecimalTypeSerializerTests : BaseTypeSerializerTests<decimal, DecimalSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { decimal.MinValue, "min" },
            new object[] { decimal.MaxValue, "max" },
            new object[] { 0.0m, "zero" },
            new object[] { 1.0m, "one" },
            new object[] { -1.0m, "minus_one" },
            new object[] { 3.1415926535897932384626433832m, "pi" },
            new object[] { -3.1415926535897932384626433832m, "minus_pi" },
            new object[] { 1234567890123456789012345678.9m, "big_pos" },
            new object[] { -1234567890123456789012345678.9m, "big_neg" },
            new object[] { 1234.5678901234567890123456789m, "rnd_pos" },
            new object[] { -1234.5678901234567890123456789m, "rnd_neg" }
        };

        public DecimalTypeSerializerTests(decimal value, string _) : base(value)
        {
        }
    }
}