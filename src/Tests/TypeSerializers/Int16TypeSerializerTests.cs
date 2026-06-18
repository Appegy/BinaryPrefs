using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class Int16TypeSerializerTests : BaseTypeSerializerTests<short, Int16Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { short.MinValue, "min" },
            new object[] { short.MaxValue, "max" },
            new object[] { (short)0, "zero" },
            new object[] { (short)1, "one" },
            new object[] { (short)-1, "minus_one" },
            new object[] { (short)1024, "pow2" },
            new object[] { (short)-1024, "minus_pow2" },
            new object[] { (short)12345, "rnd_pos" },
            new object[] { (short)-12345, "rnd_neg" }
        };

        public Int16TypeSerializerTests(short value, string _) : base(value)
        {
        }
    }
}