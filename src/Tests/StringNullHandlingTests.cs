using System;
using System.IO;
using Appegy.Storage.Serializers;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage
{
    [TestFixture]
    internal class StringNullHandlingTests : BaseStorageTests
    {
        [Test]
        public void WhenNullStringWritten_ThenReadBackAsEmpty()
        {
            var buffer = new byte[256];
            using var writeStream = new MemoryStream(buffer);
            using var readStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(writeStream);
            using var reader = new BinaryReader(readStream);

            StringSerializer.Shared.WriteTo(writer, null!);
            var value = StringSerializer.Shared.ReadFrom(reader);

            value.Should().Be(string.Empty);
            writeStream.Position.Should().Be(readStream.Position);
        }

        [Test]
        public void WhenLegacyNullMarkerRead_ThenReturnsEmpty()
        {
            var buffer = new byte[256];
            using var writeStream = new MemoryStream(buffer);
            using var readStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(writeStream);
            using var reader = new BinaryReader(readStream);

            writer.Write(-1);
            writer.Flush();

            StringSerializer.Shared.ReadFrom(reader).Should().Be(string.Empty);
        }

        [Test]
        public void WhenStringSizeIsCorruptNegative_ThenReadFromThrows()
        {
            var buffer = new byte[256];
            using var writeStream = new MemoryStream(buffer);
            using var readStream = new MemoryStream(buffer);
            using var writer = new BinaryWriter(writeStream);
            using var reader = new BinaryReader(readStream);

            writer.Write(-57);
            writer.Flush();

            StringSerializer.Shared.Invoking(s => s.ReadFrom(reader)).Should().Throw<ArgumentOutOfRangeException>();
        }

        [Test]
        public void WhenListContainsNullString_ThenSaveDoesNotThrow_AndReloadsAsEmpty()
        {
            using (var storage = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SupportListsOf<string>().Build())
            {
                var list = storage.GetListOf<string>("items");
                list.Add("a");
                list.Add(null!);
                list.Add("b");

                storage.Invoking(s => s.Save()).Should().NotThrow();
            }

            using var reopened = BinaryStorage.Construct(StoragePath).AddPrimitiveTypes().SupportListsOf<string>().Build();
            reopened.GetReadOnlyListOf<string>("items").Should().Equal("a", string.Empty, "b");
        }
    }
}
