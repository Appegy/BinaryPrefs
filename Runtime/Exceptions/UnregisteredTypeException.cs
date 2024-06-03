using System;

namespace Appegy.BinaryStorage
{
    public class UnregisteredTypeException : Exception
    {
        public UnregisteredTypeException(Type type)
            : base($"Unregistered type in storage: {type.Name}. Did you forget to add it during storage initialization?")
        {
        }
    }
}