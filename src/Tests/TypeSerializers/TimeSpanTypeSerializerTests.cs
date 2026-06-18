using System;
using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class TimeSpanTypeSerializerTests : BaseTypeSerializerTests<TimeSpan, TimeSpanSerializer>
    {
        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { TimeSpan.Zero, "zero" },
            new object[] { TimeSpan.FromHours(1), "hour" },
            new object[] { TimeSpan.FromMinutes(30), "min_30" },
            new object[] { TimeSpan.FromSeconds(45), "sec_45" },
            new object[] { TimeSpan.FromMilliseconds(500), "ms_500" },
            new object[] { TimeSpan.FromTicks(123456789), "ticks_123456789" },
            new object[] { TimeSpan.FromDays(2), "days_2" },
            new object[] { TimeSpan.FromDays(-2), "days_minus_2" },
            new object[] { TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59) + TimeSpan.FromSeconds(59), "23_59_59" },
            new object[] { TimeSpan.FromHours(-23) + TimeSpan.FromMinutes(-59) + TimeSpan.FromSeconds(-59), "minus_23_59_59" }
        };

        public TimeSpanTypeSerializerTests(TimeSpan value, string _) : base(value)
        {
        }
    }
}