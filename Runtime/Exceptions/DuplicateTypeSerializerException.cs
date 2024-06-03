using System;
using System.IO;

namespace Appegy.BinaryStorage
{
    public class DuplicateTypeSerializerException : Exception
    {
        public DuplicateTypeSerializerException(Type type, string typeNameCode, string storagePath)
            : base($"You're trying to add second serializer for type {type.Name} ({typeNameCode}) to storage {Path.GetFileName(storagePath)}. This is not allowed")
        {
        }
    }
}