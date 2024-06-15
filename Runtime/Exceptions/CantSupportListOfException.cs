using System;

namespace Appegy.BinaryStorage
{
    public class CantSupportListOfException : Exception
    {
        public Type ItemType { get; }

        public CantSupportListOfException(Type itemType)
            : base($"Add serializer for {itemType.FullName} using {nameof(BinaryPrefs.Builder.AddTypeSerializer)} before adding support for List<{itemType.Name}>.")
        {
            ItemType = itemType;
        }
    }
}