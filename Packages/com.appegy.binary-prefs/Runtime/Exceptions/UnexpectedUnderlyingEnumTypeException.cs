using System;

namespace Appegy.Storage
{
    public class UnexpectedUnderlyingEnumTypeException : Exception
    {
        public Type EnumType { get; }
        public Type UnderlyingType { get; }

        public UnexpectedUnderlyingEnumTypeException(Type enumType, Type underlyingType)
            : base($"Unexpected underlying type of enum {enumType.FullName} - {underlyingType.FullName}")
        {
            EnumType = enumType;
            UnderlyingType = underlyingType;
        }
    }
}