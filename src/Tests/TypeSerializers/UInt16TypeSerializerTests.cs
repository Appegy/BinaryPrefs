using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class UInt16TypeSerializerTests : BaseTypeSerializerTests<ushort, UInt16Serializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { ushort.MinValue, "min" },
            new object[] { ushort.MaxValue, "max" },
            new object[] { (ushort)0, "zero" },
            new object[] { (ushort)1, "one" },
            new object[] { (ushort)1024, "pow2" },
            new object[] { (ushort)12345, "rnd" }
        };

        public UInt16TypeSerializerTests(ushort value, string _) : base(value)
        {
        }
    }
}
