﻿using System;
using NUnit.Framework;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    internal class DateTimeTypeSerializerTests : BaseTypeSerializerTests<DateTime, DateTimeSerializer>
    {
        private static DateTime[] Inputs => new[]
        {
            DateTime.MinValue, // 0001-01-01 00:00:00
            DateTime.MaxValue, // 9999-12-31 23:59:59
            new DateTime(638542591551251841L, DateTimeKind.Local),
            new DateTime(638542519494481194L, DateTimeKind.Utc),
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 6, 17, 0, 0, 0, DateTimeKind.Local),
            new DateTime(2023, 6, 1, 12, 0, 0), // 2023-06-01 12:00:00
            new DateTime(1995, 7, 26, 8, 0, 0), // 1995-07-26 08:00:00
            new DateTime(2030, 1, 15, 17, 30, 0), // 2030-01-15 17:30:00
            new DateTime(1980, 5, 10, 3, 45, 0), // 1980-05-10 03:45:00
            new DateTime(2024, 12, 31, 23, 59, 59), // 2024-12-31 23:59:59
        };

        public DateTimeTypeSerializerTests(DateTime value) : base(value)
        {
        }
    }
}