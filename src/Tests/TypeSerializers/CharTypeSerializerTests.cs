using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class CharTypeSerializerTests : BaseTypeSerializerTests<char, CharSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { '\t', "tab" },
            new object[] { '\n', "newline" },
            new object[] { '\r', "carriage_return" },
            new object[] { '\u00E9', "unicode_e" },
            new object[] { '\u20AC', "euro" },
            new object[] { '\u3042', "hiragana_a" },
            new object[] { '\u0001', "ctrl_soh" },
            new object[] { '\u4E9C', "han_1" },
            new object[] { 'a', "a" },
            new object[] { 'Z', "Z" },
            new object[] { '5', "5" },
            new object[] { ' ', "space" },
            new object[] { '!', "exclamation" },
            new object[] { 'g', "g" },
            new object[] { 'ў', "cyrillic" },
            new object[] { '里', "chinese_simple" },
            new object[] { '爾', "chinese_trad" },
            new object[] { 'ة', "arabic" }
        };

        public CharTypeSerializerTests(char value, string _) : base(value)
        {
        }
    }
}
