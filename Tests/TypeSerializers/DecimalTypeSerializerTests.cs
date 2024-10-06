using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class DecimalTypeSerializerTests : BaseTypeSerializerTests<decimal, DecimalSerializer>
    {
        private static decimal[] Inputs => new[]
        {
            decimal.MinValue, // -79228162514264337593543950335
            decimal.MaxValue, // 79228162514264337593543950335
            0.0m, // zero
            1.0m, // smallest positive decimal
            -1.0m, // smallest negative decimal
            3.1415926535897932384626433832m, // pi
            -3.1415926535897932384626433832m, // negative pi
            1234567890123456789012345678.9m, // random positive value
            -1234567890123456789012345678.9m, // random negative value
            1234.5678901234567890123456789m, // random positive value
            -1234.5678901234567890123456789m // random negative value
        };

        public DecimalTypeSerializerTests(decimal value) : base(value)
        {
        }
    }
}