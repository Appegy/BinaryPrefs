using System;
using System.Diagnostics;
using System.IO;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace Appegy.Storage.Benchmarks
{
    public static class StorageBench
    {
        public const string Tag = "BPBENCH";

        private const int WarmupIterations = 5;
        private const int ControlPayloadSize = 3344;
        private const int ReadAllocationIterations = 50_000;
        private const int ChangeAllocationIterations = 5_000;
        private const int SaveAllocationIterations = 10;

        private static int _counter;

        public static string Prepare(string root)
        {
            var directory = Path.Combine(root, "bench");
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
            Directory.CreateDirectory(directory);
            Debug.Log($"{Tag} begin root={directory} warmup={WarmupIterations}");
            return directory;
        }

        public static double RunControl(string directory)
        {
            var path = Path.Combine(directory, "control.bin");
            var payload = new byte[ControlPayloadSize];
            new System.Random(1).NextBytes(payload);
            var control = Measure(() =>
            {
                using var stream = new FileStream(path, FileMode.Create);
                stream.Write(payload, 0, payload.Length);
            }, 50);
            Debug.Log($"{Tag} control write {ControlPayloadSize}b: p50 {control.P50:F3} p90 {control.P90:F3} p99 {control.P99:F3}");
            return control.P50;
        }

        public static void RunScenario(string directory, StorageBenchScenario scenario, double controlP50)
        {
            var path = Path.Combine(directory, $"bench_{scenario.Name}.bin");

            using (var storage = Create(path))
            {
                Fill(storage, scenario);
                storage.Save();

                var fileSize = new FileInfo(path).Length;
                var keys = storage.Keys.Count;
                var save = Measure(storage.Save, scenario.Repeats);
                Debug.Log($"{Tag} {scenario.Name} keys={keys} size={fileSize}b save p50 {save.P50:F3} p90 {save.P90:F3} p99 {save.P99:F3} x{save.P50 / controlP50:F1}");

                var counter = 0;
                storage.AutoSave = true;
                var change = Measure(() => storage.Set("u00000_int", ++counter), scenario.Repeats);
                storage.AutoSave = false;
                storage.Save();
                Debug.Log($"{Tag} {scenario.Name} change blocking p50 {change.P50:F3} p90 {change.P90:F3} p99 {change.P99:F3}");

                MeasureReadAllocations(scenario, storage);
                MeasureChangeAllocations(scenario, storage);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var load = Measure(() => Create(path).Dispose(), scenario.Repeats);
            Debug.Log($"{Tag} {scenario.Name} load p50 {load.P50:F3} p90 {load.P90:F3} p99 {load.P99:F3}");
        }

        public static void RunAll(string root)
        {
            var directory = Prepare(root);
            var control = RunControl(directory);
            foreach (var scenario in StorageBenchScenario.All)
            {
                RunScenario(directory, scenario, control);
            }
            Debug.Log($"{Tag} end");
        }

        private static void MeasureReadAllocations(StorageBenchScenario scenario, BinaryStorage storage)
        {
            var has = AllocationsPerCall(() => storage.Has("u00000_int"), ReadAllocationIterations);
            var getInt = AllocationsPerCall(() => storage.Get<int>("u00000_int"), ReadAllocationIterations);
            var getString = AllocationsPerCall(() => storage.Get<string>("u00000_str"), ReadAllocationIterations);
            var typeOf = AllocationsPerCall(() => storage.TypeOf("u00000_int"), ReadAllocationIterations);
            var getList = AllocationsPerCall(() => storage.GetListOf<int>("u00000_list"), ReadAllocationIterations);
            var getDictionary = AllocationsPerCall(() => storage.GetDictionaryOf<string, int>("u00000_dict"), ReadAllocationIterations);

            var list = storage.GetListOf<int>("u00000_list");
            var enumerate = AllocationsPerCall(() =>
            {
                var sum = 0;
                for (var i = 0; i < list.Count; i++)
                {
                    sum += list[i];
                }
            }, ReadAllocationIterations);

            Debug.Log($"{Tag} {scenario.Name} read alloc b/call: has {has} get_int {getInt} get_str {getString} type_of {typeOf} get_list {getList} get_dict {getDictionary} list_read {enumerate}");
        }

        private static void MeasureChangeAllocations(StorageBenchScenario scenario, BinaryStorage storage)
        {
            var set = AllocationsPerCall(() => storage.Set("u00000_int", _counter++), ChangeAllocationIterations);
            var list = storage.GetListOf<int>("u00000_list");
            var listWrite = AllocationsPerCall(() => list[0] = _counter++, ChangeAllocationIterations);
            var setAndSave = AllocationsPerCall(() =>
            {
                storage.Set("u00000_int", _counter++);
                storage.Save();
            }, SaveAllocationIterations);

            Debug.Log($"{Tag} {scenario.Name} change alloc b/call: set {set} list_write {listWrite} set_and_save {setAndSave}");
        }

        private static BinaryStorage Create(string path)
        {
            return BinaryStorage.Construct(path)
                .AddPrimitiveTypes()
                .SupportEnum<BenchEnum>()
                .SupportListsOf<int>()
                .SupportListsOf<string>()
                .SupportSetsOf<int>()
                .SupportDictionariesOf<string, int>()
                .Build();
        }

        private static void Fill(BinaryStorage storage, StorageBenchScenario scenario)
        {
            var value = new string('x', scenario.StringLength);
            using (storage.MultipleChangeScope())
            {
                for (var unit = 0; unit < scenario.Units; unit++)
                {
                    var prefix = $"u{unit:D5}_";
                    storage.Set(prefix + "int", unit);
                    storage.Set(prefix + "bool", (unit & 1) == 0);
                    storage.Set(prefix + "float", unit * 1.5f);
                    storage.Set(prefix + "str", value);
                    storage.Set(prefix + "enum", (BenchEnum)(unit % 4));

                    var numbers = storage.GetListOf<int>(prefix + "list");
                    var strings = storage.GetListOf<string>(prefix + "strlist");
                    var set = storage.GetSetOf<int>(prefix + "set");
                    var map = storage.GetDictionaryOf<string, int>(prefix + "dict");
                    for (var i = 0; i < scenario.CollectionLength; i++)
                    {
                        numbers.Add(unit + i);
                        strings.Add($"v{i:D3}");
                        set.Add(unit * scenario.CollectionLength + i);
                        map.Add($"k{i:D3}", unit + i);
                    }
                }
            }
        }

        private static Percentiles Measure(Action action, int repeats)
        {
            for (var i = 0; i < WarmupIterations; i++)
            {
                action();
            }

            var samples = new double[repeats];
            for (var i = 0; i < repeats; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                action();
                stopwatch.Stop();
                samples[i] = stopwatch.Elapsed.TotalMilliseconds;
            }
            Array.Sort(samples);

            return new Percentiles(samples);
        }

        private static long AllocationsPerCall(Action action, int iterations)
        {
            action();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var before = Profiler.GetMonoUsedSizeLong();
            for (var i = 0; i < iterations; i++)
            {
                action();
            }
            var after = Profiler.GetMonoUsedSizeLong();

            return (after - before) / iterations;
        }

        private readonly struct Percentiles
        {
            public readonly double P50;
            public readonly double P90;
            public readonly double P99;

            public Percentiles(double[] sortedSamples)
            {
                P50 = sortedSamples[sortedSamples.Length / 2];
                P90 = sortedSamples[(int)(sortedSamples.Length * 0.9)];
                P99 = sortedSamples[(int)(sortedSamples.Length * 0.99)];
            }
        }
    }
}
