using System.IO;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    public class SerializerReadTruncationTests
    {
        [Test]
        public void StringSerializer_WhenBodyTruncated_ThenThrows()
        {
            var data = new byte[] { 10, 0, 0, 0, 1, 2, 3 };

            FluentActions.Invoking(() => ReadString(data)).Should().Throw<EndOfStreamException>();
        }

        [Test]
        public void CharSerializer_WhenBodyTruncated_ThenThrows()
        {
            var data = new byte[] { 1 };

            FluentActions.Invoking(() => ReadChar(data)).Should().Throw<EndOfStreamException>();
        }

        private static void ReadString(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            StringSerializer.Shared.ReadFrom(reader);
        }

        private static void ReadChar(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            CharSerializer.Shared.ReadFrom(reader);
        }
    }
}
