using System;
using System.IO;

namespace Appegy.Storage
{
    /// <summary>
    /// Thrown when a serializer with a TypeName that is already in use is being registered.
    /// </summary>
    public class DuplicateTypeSerializerNameException : Exception
    {
        public DuplicateTypeSerializerNameException(TypeSerializer newSerializer, TypeSerializer existingSerializer, string storagePath)
            : base($"TypeName collision detected in '{Path.GetFileName(storagePath)}'. " +
                   $"Attempted: {newSerializer.GetType().Name} (TypeName: '{newSerializer.TypeName}'); " +
                   $"Conflicting: {existingSerializer.GetType().Name} (TypeName: '{existingSerializer.TypeName}').")
        {
        }
    }
}