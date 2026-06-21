using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace Appegy.Storage.CollisionA
{
    public struct Thing
    {
        public int Value;
    }
}

namespace Appegy.Storage.CollisionB
{
    public struct Thing
    {
        public int Value;
    }
}

namespace Appegy.Storage
{
    public class TypeNameCollisionTests : BaseStorageTests
    {
        private class ThingASerializer : TypeSerializer<CollisionA.Thing>
        {
            public override bool Equals(CollisionA.Thing a, CollisionA.Thing b) => a.Value == b.Value;
            public override void WriteTo(BinaryWriter writer, CollisionA.Thing value) => writer.Write(value.Value);
            public override CollisionA.Thing ReadFrom(BinaryReader reader) => new CollisionA.Thing { Value = reader.ReadInt32() };
            public override CollisionA.Thing GetDefault() => default;
        }

        private class ThingBSerializer : TypeSerializer<CollisionB.Thing>
        {
            public override bool Equals(CollisionB.Thing a, CollisionB.Thing b) => a.Value == b.Value;
            public override void WriteTo(BinaryWriter writer, CollisionB.Thing value) => writer.Write(value.Value);
            public override CollisionB.Thing ReadFrom(BinaryReader reader) => new CollisionB.Thing { Value = reader.ReadInt32() };
            public override CollisionB.Thing GetDefault() => default;
        }

        [Test]
        public void WhenTwoDictionaryValueTypesShareShortName_ThenBothRegisterAndRoundTrip()
        {
            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddPrimitiveTypes()
                       .AddTypeSerializer(new ThingASerializer())
                       .AddTypeSerializer(new ThingBSerializer())
                       .SupportDictionariesOf<int, CollisionA.Thing>()
                       .SupportDictionariesOf<int, CollisionB.Thing>()
                       .Build())
            {
                storage.GetDictionaryOf<int, CollisionA.Thing>("a")[1] = new CollisionA.Thing { Value = 10 };
                storage.GetDictionaryOf<int, CollisionB.Thing>("b")[1] = new CollisionB.Thing { Value = 20 };
                storage.Save();
            }

            using var reopened = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .AddTypeSerializer(new ThingASerializer())
                .AddTypeSerializer(new ThingBSerializer())
                .SupportDictionariesOf<int, CollisionA.Thing>()
                .SupportDictionariesOf<int, CollisionB.Thing>()
                .Build();

            reopened.GetReadOnlyDictionaryOf<int, CollisionA.Thing>("a")[1].Value.Should().Be(10);
            reopened.GetReadOnlyDictionaryOf<int, CollisionB.Thing>("b")[1].Value.Should().Be(20);
        }

        [Test]
        public void WhenDictionarySectionHasLegacyShortName_ThenLoadsViaFallback()
        {
            using (var storage = BinaryStorage.Construct(StoragePath)
                       .AddPrimitiveTypes()
                       .SupportDictionariesOf<string, int>()
                       .Build())
            {
                storage.GetDictionaryOf<string, int>("d")["x"] = 5;
                storage.Save();
            }

            RewriteSectionName(StoragePath, name => name.Contains("ReactiveDictionary"), "ReactiveDictionary`2<String:Int32>");

            using var reopened = BinaryStorage.Construct(StoragePath)
                .AddPrimitiveTypes()
                .SupportDictionariesOf<string, int>()
                .Build();

            reopened.GetReadOnlyDictionaryOf<string, int>("d")["x"].Should().Be(5);
        }

        private static void RewriteSectionName(string path, Func<string, bool> match, string replacement)
        {
            string version;
            long reserved;
            string[] names;
            var records = new List<(string Key, int TypeIndex, byte[] Value)>();

            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                version = reader.ReadString();
                reserved = reader.ReadInt64();
                var serializersCount = reader.ReadInt32();
                names = new string[serializersCount];
                for (var i = 0; i < serializersCount; i++)
                {
                    names[i] = reader.ReadString();
                }
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var key = reader.ReadString();
                    var typeIndex = reader.ReadInt32();
                    var size = reader.ReadInt64();
                    var value = reader.ReadBytes((int)size);
                    records.Add((key, typeIndex, value));
                }
            }

            for (var i = 0; i < names.Length; i++)
            {
                if (match(names[i]))
                {
                    names[i] = replacement;
                }
            }

            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            using (var writer = new BinaryWriter(stream, Encoding.UTF8))
            {
                writer.Write(version);
                writer.Write(reserved);
                writer.Write(names.Length);
                foreach (var name in names)
                {
                    writer.Write(name);
                }
                writer.Write(records.Count);
                foreach (var record in records)
                {
                    writer.Write(record.Key);
                    writer.Write(record.TypeIndex);
                    writer.Write((long)record.Value.Length);
                    writer.Write(record.Value);
                }
            }
        }
    }
}
