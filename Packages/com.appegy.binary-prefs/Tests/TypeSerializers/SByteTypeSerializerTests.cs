using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class SByteTypeSerializerTests : BaseTypeSerializerTests<sbyte, SByteSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { sbyte.MinValue, "min" },
            new object[] { sbyte.MaxValue, "max" },
            new object[] { (sbyte)0, "zero" },
            new object[] { (sbyte)1, "one" },
            new object[] { (sbyte)-1, "minus_one" },
            new object[] { (sbyte)64, "pow2" },
            new object[] { (sbyte)-64, "minus_pow2" },
            new object[] { (sbyte)10, "rnd_pos" },
            new object[] { (sbyte)-10, "rnd_neg" }
        };

        public SByteTypeSerializerTests(sbyte value, string _) : base(value)
        {
        }
    }
}