using NUnit.Framework;
using System.Collections.Generic;
using Appegy.Storage.Serializers;

namespace Appegy.Storage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumByteTypeSerializerTests : TypeSerializerTests<EnumByteTypeSerializerTests.ByteEnum, EnumTypeSerializer<EnumByteTypeSerializerTests.ByteEnum, byte>>
    {
        public enum ByteEnum : byte { Value1, Value2, Value3 }

        public EnumByteTypeSerializerTests(ByteEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<ByteEnum, byte>(ByteSerializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { ByteEnum.Value1, "v1" },
            new object[] { ByteEnum.Value2, "v2" },
            new object[] { ByteEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumSByteTypeSerializerTests : TypeSerializerTests<EnumSByteTypeSerializerTests.SByteEnum, EnumTypeSerializer<EnumSByteTypeSerializerTests.SByteEnum, sbyte>>
    {
        public enum SByteEnum : sbyte { Value1, Value2, Value3 }

        public EnumSByteTypeSerializerTests(SByteEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<SByteEnum, sbyte>(SByteSerializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { SByteEnum.Value1, "v1" },
            new object[] { SByteEnum.Value2, "v2" },
            new object[] { SByteEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumShortTypeSerializerTests : TypeSerializerTests<EnumShortTypeSerializerTests.ShortEnum, EnumTypeSerializer<EnumShortTypeSerializerTests.ShortEnum, short>>
    {
        public enum ShortEnum : short { Value1, Value2, Value3 }

        public EnumShortTypeSerializerTests(ShortEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<ShortEnum, short>(Int16Serializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { ShortEnum.Value1, "v1" },
            new object[] { ShortEnum.Value2, "v2" },
            new object[] { ShortEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumUShortTypeSerializerTests : TypeSerializerTests<EnumUShortTypeSerializerTests.UShortEnum, EnumTypeSerializer<EnumUShortTypeSerializerTests.UShortEnum, ushort>>
    {
        public enum UShortEnum : ushort { Value1, Value2, Value3 }

        public EnumUShortTypeSerializerTests(UShortEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<UShortEnum, ushort>(UInt16Serializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { UShortEnum.Value1, "v1" },
            new object[] { UShortEnum.Value2, "v2" },
            new object[] { UShortEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumIntTypeSerializerTests : TypeSerializerTests<EnumIntTypeSerializerTests.IntEnum, EnumTypeSerializer<EnumIntTypeSerializerTests.IntEnum, int>>
    {
        public enum IntEnum : int { Value1, Value2, Value3 }

        public EnumIntTypeSerializerTests(IntEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<IntEnum, int>(Int32Serializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { IntEnum.Value1, "v1" },
            new object[] { IntEnum.Value2, "v2" },
            new object[] { IntEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumUIntTypeSerializerTests : TypeSerializerTests<EnumUIntTypeSerializerTests.UIntEnum, EnumTypeSerializer<EnumUIntTypeSerializerTests.UIntEnum, uint>>
    {
        public enum UIntEnum : uint { Value1, Value2, Value3 }

        public EnumUIntTypeSerializerTests(UIntEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<UIntEnum, uint>(UInt32Serializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { UIntEnum.Value1, "v1" },
            new object[] { UIntEnum.Value2, "v2" },
            new object[] { UIntEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumLongTypeSerializerTests : TypeSerializerTests<EnumLongTypeSerializerTests.LongEnum, EnumTypeSerializer<EnumLongTypeSerializerTests.LongEnum, long>>
    {
        public enum LongEnum : long { Value1, Value2, Value3 }

        public EnumLongTypeSerializerTests(LongEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<LongEnum, long>(Int64Serializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { LongEnum.Value1, "v1" },
            new object[] { LongEnum.Value2, "v2" },
            new object[] { LongEnum.Value3, "v3" }
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumULongTypeSerializerTests : TypeSerializerTests<EnumULongTypeSerializerTests.ULongEnum, EnumTypeSerializer<EnumULongTypeSerializerTests.ULongEnum, ulong>>
    {
        public enum ULongEnum : ulong { Value1, Value2, Value3 }

        public EnumULongTypeSerializerTests(ULongEnum defaultValue, string _)
            : base(defaultValue, new EnumTypeSerializer<ULongEnum, ulong>(UInt64Serializer.Shared, false)) { }

        private static IEnumerable<object[]> Inputs => new[]
        {
            new object[] { ULongEnum.Value1, "v1" },
            new object[] { ULongEnum.Value2, "v2" },
            new object[] { ULongEnum.Value3, "v3" }
        };
    }
}
