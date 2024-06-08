using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumByteTypeSerializerTests : TypeSerializerTests<EnumByteTypeSerializerTests.ByteEnum, EnumSerializer<EnumByteTypeSerializerTests.ByteEnum, byte>>
    {
        public enum ByteEnum : byte
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumByteTypeSerializerTests(ByteEnum defaultValue)
            : base(defaultValue, new EnumSerializer<ByteEnum, byte>(ByteSerializer.Shared, false))
        {
        }

        private static ByteEnum[] Inputs => new[]
        {
            ByteEnum.Value1,
            ByteEnum.Value2,
            ByteEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumSByteTypeSerializerTests : TypeSerializerTests<EnumSByteTypeSerializerTests.SByteEnum, EnumSerializer<EnumSByteTypeSerializerTests.SByteEnum, sbyte>>
    {
        public enum SByteEnum : sbyte
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumSByteTypeSerializerTests(SByteEnum defaultValue)
            : base(defaultValue, new EnumSerializer<SByteEnum, sbyte>(SByteSerializer.Shared, false))
        {
        }

        private static SByteEnum[] Inputs => new[]
        {
            SByteEnum.Value1,
            SByteEnum.Value2,
            SByteEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumShortTypeSerializerTests : TypeSerializerTests<EnumShortTypeSerializerTests.ShortEnum, EnumSerializer<EnumShortTypeSerializerTests.ShortEnum, short>>
    {
        public enum ShortEnum : short
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumShortTypeSerializerTests(ShortEnum defaultValue)
            : base(defaultValue, new EnumSerializer<ShortEnum, short>(Int16Serializer.Shared, false))
        {
        }

        private static ShortEnum[] Inputs => new[]
        {
            ShortEnum.Value1,
            ShortEnum.Value2,
            ShortEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumUShortTypeSerializerTests : TypeSerializerTests<EnumUShortTypeSerializerTests.UShortEnum, EnumSerializer<EnumUShortTypeSerializerTests.UShortEnum, ushort>>
    {
        public enum UShortEnum : ushort
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumUShortTypeSerializerTests(UShortEnum defaultValue)
            : base(defaultValue, new EnumSerializer<UShortEnum, ushort>(UInt16Serializer.Shared, false))
        {
        }

        private static UShortEnum[] Inputs => new[]
        {
            UShortEnum.Value1,
            UShortEnum.Value2,
            UShortEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumIntTypeSerializerTests : TypeSerializerTests<EnumIntTypeSerializerTests.IntEnum, EnumSerializer<EnumIntTypeSerializerTests.IntEnum, int>>
    {
        public enum IntEnum : int
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumIntTypeSerializerTests(IntEnum defaultValue)
            : base(defaultValue, new EnumSerializer<IntEnum, int>(Int32Serializer.Shared, false))
        {
        }

        private static IntEnum[] Inputs => new[]
        {
            IntEnum.Value1,
            IntEnum.Value2,
            IntEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumUIntTypeSerializerTests : TypeSerializerTests<EnumUIntTypeSerializerTests.UIntEnum, EnumSerializer<EnumUIntTypeSerializerTests.UIntEnum, uint>>
    {
        public enum UIntEnum : uint
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumUIntTypeSerializerTests(UIntEnum defaultValue)
            : base(defaultValue, new EnumSerializer<UIntEnum, uint>(UInt32Serializer.Shared, false))
        {
        }

        private static UIntEnum[] Inputs => new[]
        {
            UIntEnum.Value1,
            UIntEnum.Value2,
            UIntEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumLongTypeSerializerTests : TypeSerializerTests<EnumLongTypeSerializerTests.LongEnum, EnumSerializer<EnumLongTypeSerializerTests.LongEnum, long>>
    {
        public enum LongEnum : long
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumLongTypeSerializerTests(LongEnum defaultValue)
            : base(defaultValue, new EnumSerializer<LongEnum, long>(Int64Serializer.Shared, false))
        {
        }

        private static LongEnum[] Inputs => new[]
        {
            LongEnum.Value1,
            LongEnum.Value2,
            LongEnum.Value3
        };
    }

    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumULongTypeSerializerTests : TypeSerializerTests<EnumULongTypeSerializerTests.ULongEnum, EnumSerializer<EnumULongTypeSerializerTests.ULongEnum, ulong>>
    {
        public enum ULongEnum : ulong
        {
            Value1,
            Value2,
            Value3,
        }

        public EnumULongTypeSerializerTests(ULongEnum defaultValue)
            : base(defaultValue, new EnumSerializer<ULongEnum, ulong>(UInt64Serializer.Shared, false))
        {
        }

        private static ULongEnum[] Inputs => new[]
        {
            ULongEnum.Value1,
            ULongEnum.Value2,
            ULongEnum.Value3
        };
    }
}