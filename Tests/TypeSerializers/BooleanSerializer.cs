using NUnit.Framework;
using System.Collections.Generic;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class BooleanTypeSerializerTests : BaseTypeSerializerTests<bool, BooleanSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { true, "true" },
            new object[] { false, "false" }
        };

        public BooleanTypeSerializerTests(bool value, string _) : base(value)
        {
        }
    }
}