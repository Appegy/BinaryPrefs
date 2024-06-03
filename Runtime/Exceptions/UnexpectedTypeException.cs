using System;

namespace Appegy.BinaryStorage
{
    public class UnexpectedTypeException : Exception
    {
        public UnexpectedTypeException(string key, string action, Type actualType, Type expected)
            : base($"Current type for key '{key}' is {actualType.Name} but you trying to {action} value with type {expected.Name}")
        {
        }
    }
}