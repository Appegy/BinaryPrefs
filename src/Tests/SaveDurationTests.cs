using System.Collections.Generic;
using System.IO;
using Appegy.Storage.Serializers;
using NUnit.Framework;
using UnityEngine;

namespace Appegy.Storage
{
    [Explicit("Writes to the real disk and never asserts - numbers are hardware specific. Run manually to compare save cost.")]
    public class SaveDurationTests : BaseStorageTests
    {
        [TestCase(3, 16)]
        [TestCase(64, 512)]
        public void MeasureSaveDuration(int stringKeys, int stringLength)
        {
            var (sections, data) = CreateSample(stringKeys, stringLength);
            BinaryStorageIO.SaveDataOnDisk(StoragePath, sections, data);
            var fileSize = new FileInfo(StoragePath).Length;

            var (p50, p99) = DurationProbe.MillisecondsPerCall(() => BinaryStorageIO.SaveDataOnDisk(StoragePath, sections, data));

            Debug.Log($"Save of {fileSize}b: p50 {p50:F3}ms, p99 {p99:F3}ms");
        }

        private static (List<BinarySection> sections, Dictionary<string, Record> data) CreateSample(int stringKeys, int stringLength)
        {
            var sections = new List<BinarySection>
            {
                new TypedBinarySection<int>(Int32Serializer.Shared),
                new TypedBinarySection<string>(StringSerializer.Shared)
            };
            var data = new Dictionary<string, Record> { { "int", new Record<int>(42, 0) } };
            sections[0].Count++;
            var value = new string('x', stringLength);
            for (var i = 0; i < stringKeys; i++)
            {
                data.Add($"string_{i}", new Record<string>(value, 1));
                sections[1].Count++;
            }
            return (sections, data);
        }
    }
}
