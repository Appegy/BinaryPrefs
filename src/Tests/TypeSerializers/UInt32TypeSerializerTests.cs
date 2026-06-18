using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class UInt32TypeSerializerTests : BaseTypeSerializerTests<uint, UInt32Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { uint.MinValue, "min" },
            new object[] { uint.MaxValue, "max" },
            new object[] { 0u, "zero" },
            new object[] { 1u, "one" },
            new object[] { 1024u, "pow2" },
            new object[] { 1234567890u, "rnd" }
        };

        public UInt32TypeSerializerTests(uint value, string _) : base(value)
        {
        }
    }
}