using System;

namespace Appegy.BinaryStorage
{
    public class UnregisteredTypeException : Exception
    {
        public UnregisteredTypeException(Type type)
            : this(type.Name)
        {
        }

        public UnregisteredTypeException(string type)
            : base($"Unregistered type in storage: {type}. Did you forget to add it during storage initialization?")
        {
        }
    }
}