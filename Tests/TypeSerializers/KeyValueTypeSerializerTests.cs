using System.Collections.Generic;
using Appegy.Storage.Serializers;
using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class KeyValueTypeSerializerTests : TypeSerializerTests<KeyValuePair<string, int>, KeyValueTypeSerializer<string, int>>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { new KeyValuePair<string, int>("key1", 1), "key1_1" },
            new object[] { new KeyValuePair<string, int>("key2", 2), "key2_2" },
            new object[] { new KeyValuePair<string, int>("key3", 3), "key3_3" },
            new object[] { new KeyValuePair<string, int>("", 0), "empty_0" },
            new object[] { new KeyValuePair<string, int>(null, 0), "null_0" },
            new object[] { new KeyValuePair<string, int>("key with space", 12345), "space_12345" },
            new object[] { new KeyValuePair<string, int>("long key with multiple words", 67890), "long_67890" }
        };

        public KeyValueTypeSerializerTests(KeyValuePair<string, int> defaultValue, string _)
            : base(defaultValue, new KeyValueTypeSerializer<string, int>(StringSerializer.Shared, Int32Serializer.Shared))
        {
        }
    }
}