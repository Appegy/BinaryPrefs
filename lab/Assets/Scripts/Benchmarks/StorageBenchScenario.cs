namespace Appegy.Storage.Benchmarks
{
    public enum BenchEnum
    {
        First,
        Second,
        Third,
        Fourth
    }

    public readonly struct StorageBenchScenario
    {
        public static readonly StorageBenchScenario[] All =
        {
            new("S", 6, 64, 8, 50),
            new("M", 190, 64, 8, 50),
            new("L", 1800, 64, 8, 30),
            new("XL", 3600, 64, 8, 20)
        };

        public readonly string Name;
        public readonly int Units;
        public readonly int StringLength;
        public readonly int CollectionLength;
        public readonly int Repeats;

        public StorageBenchScenario(string name, int units, int stringLength, int collectionLength, int repeats)
        {
            Name = name;
            Units = units;
            StringLength = stringLength;
            CollectionLength = collectionLength;
            Repeats = repeats;
        }
    }
}
