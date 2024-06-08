using NUnit.Framework;

namespace Appegy.BinaryStorage.TypeSerializers
{
    [TestFixture]
    [TestFixtureSource(nameof(Inputs))]
    public class EnumByteTypeSerializerTests : TypeSerializerTests<ByteEnum, EnumSerializer<ByteEnum, byte>>
    {
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
    public class EnumSByteTypeSerializerTests : TypeSerializerTests<SByteEnum, EnumSerializer<SByteEnum, sbyte>>
    {
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
    public class EnumShortTypeSerializerTests : TypeSerializerTests<ShortEnum, EnumSerializer<ShortEnum, short>>
    {
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
    public class EnumUShortTypeSerializerTests : TypeSerializerTests<UShortEnum, EnumSerializer<UShortEnum, ushort>>
    {
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
    public class EnumIntTypeSerializerTests : TypeSerializerTests<IntEnum, EnumSerializer<IntEnum, int>>
    {
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
    public class EnumUIntTypeSerializerTests : TypeSerializerTests<UIntEnum, EnumSerializer<UIntEnum, uint>>
    {
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
    public class EnumLongTypeSerializerTests : TypeSerializerTests<LongEnum, EnumSerializer<LongEnum, long>>
    {
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
    public class EnumULongTypeSerializerTests : TypeSerializerTests<ULongEnum, EnumSerializer<ULongEnum, ulong>>
    {
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

    #region Enums

    public enum ByteEnum : byte
    {
        Value1,
        Value2,
        Value3,
    }

    public enum SByteEnum : sbyte
    {
        Value1,
        Value2,
        Value3,
    }

    public enum ShortEnum : short
    {
        Value1,
        Value2,
        Value3,
    }

    public enum UShortEnum : ushort
    {
        Value1,
        Value2,
        Value3,
    }

    public enum IntEnum : int
    {
        Value1,
        Value2,
        Value3,
    }

    public enum UIntEnum : uint
    {
        Value1,
        Value2,
        Value3,
    }

    public enum LongEnum : long
    {
        Value1,
        Value2,
        Value3,
    }

    public enum ULongEnum : ulong
    {
        Value1,
        Value2,
        Value3,
    }

    #endregion
}