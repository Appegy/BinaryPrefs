using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class StringTypeSerializerTests : BaseTypeSerializerTests<string, StringSerializer>
    {
        private static string[] Inputs => new[]
        {
            null, // null
            "", // empty
            "Hello world!", // latin
            "Прывітанне сусвет!", // cyrillic
            "你好世界", // chinese
            "مرحبا بالعالم!", // arabic
            "こんにちは世界", // japanese
            "안녕하세요 세계", // korean
            "שלום עולם!", // hebrew
            "Bonjour le monde!", // french
            "¡Hola mundo!", // spanish
            "Olá mundo!", // portuguese
            "Hallo Welt!", // german
            "Ciao mondo!", // italian
            "नमस्ते दुनिया!", // hindi
            "👋 🌍", // emojis
            "!@#$%^&*()_+-=[]{}|;':\",.<>/?", // special characters
            "    ", // whitespace
            "Line1\nLine2\nLine3", // multiline
            new string('a', 1000), // long string
            "Leading and trailing spaces ", // leading/trailing spaces
            "Mixed123Numbers456And789Text" // alphanumeric
            // TODO: this one currently breaks tests
            //"Null\0Character", // string with null character
        };

        public StringTypeSerializerTests(string defaultValue) : base(defaultValue)
        {
        }
    }
}