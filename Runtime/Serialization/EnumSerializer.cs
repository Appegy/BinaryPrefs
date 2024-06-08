using System;
using System.IO;

namespace Appegy.BinaryStorage
{
    public class EnumSerializer<TEnum, TNumber> : TypeSerializer<TEnum>
        where TEnum : unmanaged
        where TNumber : unmanaged, IEquatable<TNumber>
    {
        private readonly TypeSerializer<TNumber> _numberType;

        public override string TypeName { get; }
        public override int SizeOf(TEnum value) => _numberType.SizeOf(ToNumber(value));

        public override bool Equals(TEnum value1, TEnum value2)
        {
            return ToNumber(value1).Equals(ToNumber(value2));
        }

        public EnumSerializer(TypeSerializer<TNumber> numberType, bool useFullName)
        {
            _numberType = numberType;
            TypeName = useFullName ? typeof(TEnum).FullName : typeof(TEnum).Name;
        }

        public override void WriteTo(BinaryWriter writer, TEnum value)
        {
            var number = ToNumber(value);
            _numberType.WriteTo(writer, number);
        }

        public override TEnum ReadFrom(BinaryReader reader)
        {
            var number = _numberType.ReadFrom(reader);
            return FromNumber(number);
        }

        public static unsafe TNumber ToNumber(TEnum enumValue)
        {
            return *(TNumber*)(&enumValue);
        }

        public static unsafe TEnum FromNumber(TNumber numberValue)
        {
            return *(TEnum*)(&numberValue);
        }
    }
}