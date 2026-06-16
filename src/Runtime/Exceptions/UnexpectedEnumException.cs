using System;

namespace Appegy.Storage
{
    public class UnexpectedEnumException : Exception
    {
        public UnexpectedEnumException(Type actualType, Enum value)
            : base($"Unexpected enum type {actualType.Name}.{value}")
        {
        }
    }
}