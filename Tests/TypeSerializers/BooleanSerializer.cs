using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class BooleanTypeSerializerTests : BaseTypeSerializerTests<bool, BooleanSerializer>
    {
        private static bool[] Inputs => new[]
        {
            true, // true boolean value
            false, // false boolean value
        };

        public BooleanTypeSerializerTests(bool value) : base(value)
        {
        }
    }
}