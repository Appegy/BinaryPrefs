using System;
using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class DateTimeTypeSerializerTests : BaseTypeSerializerTests<DateTime, DateTimeSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { DateTime.MinValue, "min" },
            new object[] { DateTime.MaxValue, "max" },
            new object[] { new DateTime(638542591551251841L, DateTimeKind.Local), "ticks_local_1" },
            new object[] { new DateTime(638542519494481194L, DateTimeKind.Utc), "ticks_utc_1" },
            new object[] { new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), "epoch_utc" },
            new object[] { new DateTime(2024, 6, 17, 0, 0, 0, DateTimeKind.Local), "2024_06_17_local" },
            new object[] { new DateTime(2023, 6, 1, 12, 0, 0), "2023_06_01_12_00" },
            new object[] { new DateTime(1995, 7, 26, 8, 0, 0), "1995_07_26_08_00" },
            new object[] { new DateTime(2030, 1, 15, 17, 30, 0), "2030_01_15_17_30" },
            new object[] { new DateTime(1980, 5, 10, 3, 45, 0), "1980_05_10_03_45" },
            new object[] { new DateTime(2024, 12, 31, 23, 59, 59), "2024_12_31_23_59_59" }
        };

        public DateTimeTypeSerializerTests(DateTime value, string _) : base(value)
        {
        }
    }
}
