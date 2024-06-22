using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class DoubleTypeSerializerTests : BaseTypeSerializerTests<double, DoubleSerializer>
    {
        private static double[] Inputs => new[]
        {
            double.MinValue, // -1.7976931348623157E+308
            double.MaxValue, // 1.7976931348623157E+308
            double.Epsilon, // 4.94065645841247E-324
            double.NegativeInfinity, // Negative infinity
            double.PositiveInfinity, // Positive infinity
            0.0, // zero
            1.0, // smallest positive double
            -1.0, // smallest negative double
            3.14159265358979, // pi
            -3.14159265358979 // negative pi
        };

        public DoubleTypeSerializerTests(double value) : base(value)
        {
        }
    }
}