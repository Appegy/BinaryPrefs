using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class DoubleTypeSerializerTests : BaseTypeSerializerTests<double, DoubleSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { double.MinValue, "min" },
            new object[] { double.MaxValue, "max" },
            new object[] { double.Epsilon, "epsilon" },
            new object[] { double.NegativeInfinity, "neg_inf" },
            new object[] { double.PositiveInfinity, "pos_inf" },
            new object[] { 0.0, "zero" },
            new object[] { 1.0, "one" },
            new object[] { -1.0, "minus_one" },
            new object[] { 3.14159265358979, "pi" },
            new object[] { -3.14159265358979, "minus_pi" }
        };

        public DoubleTypeSerializerTests(double value, string _) : base(value)
        {
        }
    }
}