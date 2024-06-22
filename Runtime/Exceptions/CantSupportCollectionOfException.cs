using System;

namespace Appegy.BinaryStorage
{
    public class CantSupportCollectionOfException : Exception
    {
        public Type ItemType { get; }

        public CantSupportCollectionOfException(Type itemType)
            : base($"Add serializer for {itemType.FullName} using {nameof(BinaryPrefs.Builder.AddTypeSerializer)} before adding support for List<{itemType.Name}>.")
        {
            ItemType = itemType;
        }
    }
}