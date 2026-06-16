using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class SingleTypeSerializerTests : BaseTypeSerializerTests<float, SingleSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { float.MinValue, "min" },
            new object[] { float.MaxValue, "max" },
            new object[] { float.Epsilon, "epsilon" },
            new object[] { float.NegativeInfinity, "neg_inf" },
            new object[] { float.PositiveInfinity, "pos_inf" },
            new object[] { 0f, "zero" },
            new object[] { 1f, "one" },
            new object[] { -1f, "minus_one" },
            new object[] { 3.14159f, "pi" },
            new object[] { -3.14159f, "minus_pi" }
        };

        public SingleTypeSerializerTests(float value, string _) : base(value)
        {
        }
    }
}