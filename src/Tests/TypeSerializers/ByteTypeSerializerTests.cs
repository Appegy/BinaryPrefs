using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class ByteTypeSerializerTests : BaseTypeSerializerTests<byte, ByteSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { byte.MinValue, "min" },
            new object[] { byte.MaxValue, "max" },
            new object[] { (byte)1, "one" },
            new object[] { (byte)127, "mid" },
            new object[] { (byte)64, "pow2" },
            new object[] { (byte)3, "below_pow2" },
            new object[] { (byte)10, "small" },
            new object[] { (byte)100, "middle" },
            new object[] { (byte)200, "large" }
        };

        public ByteTypeSerializerTests(byte value, string _) : base(value)
        {
        }
    }
}