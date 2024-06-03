using System;

namespace Appegy.BinaryStorage
{
    public class UnexpectedTypeException : Exception
    {
        public UnexpectedTypeException(string key, string action, Type expectedType, Type actualType)
            : base($"Current type for key '{key}' is {expectedType.Name} but you trying to {action} value with type {actualType.Name}")
        {
        }
    }
}