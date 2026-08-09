using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class StringTypeSerializerTests : BaseTypeSerializerTests<string, StringSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { null, "null" },
            new object[] { "", "empty" },
            new object[] { "Hello world!", "latin" },
            new object[] { "Прывітанне сусвет!", "cyrillic" },
            new object[] { "你好世界", "chinese" },
            new object[] { "مرحبا بالعالم!", "arabic" },
            new object[] { "こんにちは世界", "japanese" },
            new object[] { "안녕하세요 세계", "korean" },
            new object[] { "שלום עולם!", "hebrew" },
            new object[] { "Bonjour le monde!", "french" },
            new object[] { "¡Hola mundo!", "spanish" },
            new object[] { "Olá mundo!", "portuguese" },
            new object[] { "Hallo Welt!", "german" },
            new object[] { "Ciao mondo!", "italian" },
            new object[] { "नमस्ते दुनिया!", "hindi" },
            new object[] { "👋 🌍", "emojis" },
            new object[] { "!@#$%^&*()_+-=[]{}|;':\",.<>/?", "special" },
            new object[] { "    ", "whitespace" },
            new object[] { "Line1\nLine2\nLine3", "multiline" },
            new object[] { new string('a', 1000), "long" },
            new object[] { "Leading and trailing spaces ", "spaces" },
            new object[] { "Mixed123Numbers456And789Text", "alphanumeric" }
        };

        public StringTypeSerializerTests(string defaultValue, string _) : base(defaultValue)
        {
        }
    }
}
