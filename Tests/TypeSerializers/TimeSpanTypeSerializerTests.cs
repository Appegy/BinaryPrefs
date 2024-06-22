using NUnit.Framework;
using System;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class TimeSpanTypeSerializerTests : BaseTypeSerializerTests<TimeSpan, TimeSpanSerializer>
    {
        private static TimeSpan[] Inputs => new[]
        {
            TimeSpan.Zero, // 00:00:00
            TimeSpan.FromHours(1), // 01:00:00
            TimeSpan.FromMinutes(30), // 00:30:00
            TimeSpan.FromSeconds(45), // 00:00:45
            TimeSpan.FromMilliseconds(500), // 00:00:00.5000000
            TimeSpan.FromTicks(123456789), // 00:00:00.0123456
            TimeSpan.FromDays(2), // 2.00:00:00
            TimeSpan.FromDays(-2), // -2.00:00:00
            TimeSpan.FromHours(23) + TimeSpan.FromMinutes(59) + TimeSpan.FromSeconds(59), // 23:59:59
            TimeSpan.FromHours(-23) + TimeSpan.FromMinutes(-59) + TimeSpan.FromSeconds(-59), // -23:59:59
        };

        public TimeSpanTypeSerializerTests(TimeSpan value) : base(value)
        {
        }
    }
}