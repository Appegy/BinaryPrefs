using System.Collections.Generic;
using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class KeyValueTypeSerializerTests : TypeSerializerTests<KeyValuePair<string, int>, KeyValueTypeSerializer<string, int>>
    {
        public KeyValueTypeSerializerTests(KeyValuePair<string, int> defaultValue)
            : base(defaultValue, new KeyValueTypeSerializer<string, int>(StringSerializer.Shared, Int32Serializer.Shared))
        {
        }

        private static KeyValuePair<string, int>[] Inputs => new[]
        {
            new KeyValuePair<string, int>("key1", 1),
            new KeyValuePair<string, int>("key2", 2),
            new KeyValuePair<string, int>("key3", 3),
            new KeyValuePair<string, int>("", 0), // empty key
            new KeyValuePair<string, int>(null, 0), // null key
            new KeyValuePair<string, int>("key with space", 12345),
            new KeyValuePair<string, int>("long key with multiple words", 67890),
        };
    }
}