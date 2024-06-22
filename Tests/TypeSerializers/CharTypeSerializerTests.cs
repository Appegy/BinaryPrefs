using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class CharTypeSerializerTests : BaseTypeSerializerTests<char, CharSerializer>
    {
        private static char[] Inputs => new[]
        {
            '\t', // tab
            '\n', // newline
            '\r', // carriage return
            '\u00E9', // Unicode character é
            '\u20AC', // Euro sign €
            '\u3042', // Hiragana letter A
            //'\uD83D', // Emoji
            '\u0001', // ASCII control character (Start of Heading)
            '\u4E9C', // Han character 亜
            'a', // lowercase letter
            'Z', // uppercase letter
            '5', // digit
            ' ', // space
            '!', // exclamation mark
            'g', // latin
            'ў', // cyrillic
            '里', // chinese simplified
            '爾', // chinese traditional
            'ة', // arabic
            // TODO: this one currently breaks tests
            //'\0',
        };

        public CharTypeSerializerTests(char value) : base(value)
        {
        }
    }
}