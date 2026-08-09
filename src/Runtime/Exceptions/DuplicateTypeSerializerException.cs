using System;
using System.IO;

namespace Appegy.Storage
{
    /// <summary>
    /// Thrown when a second serializer for the same value type is being registered.
    /// </summary>
    public class DuplicateTypeSerializerException : Exception
    {
        public DuplicateTypeSerializerException(TypeSerializer newSerializer, TypeSerializer existingSerializer, string storagePath)
            : base($"Duplicate serializer detected in '{Path.GetFileName(storagePath)}'. " +
                   $"Attempted: {newSerializer.GetType().Name} (TypeName: '{newSerializer.TypeName}'); " +
                   $"Already registered: {existingSerializer.GetType().Name} (TypeName: '{existingSerializer.TypeName}').")
        {
        }
    }
}
