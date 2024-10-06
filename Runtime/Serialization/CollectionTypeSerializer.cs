using System.Collections.Generic;
using System.IO;

namespace Appegy.Storage
{
    internal class CollectionTypeSerializer<T, TCollection> : TypeSerializer<TCollection>
        where TCollection : class, IReactiveCollection, ICollection<T>, new()
    {
        private readonly TypeSerializer<T> _typeSerializer;

        public override string TypeName { get; }

        public CollectionTypeSerializer(TypeSerializer<T> typeSerializer)
        {
            _typeSerializer = typeSerializer;
            TypeName = $"{typeof(TCollection).Name}<{typeSerializer.TypeName}>";
        }

        public override bool Equals(TCollection value1, TCollection value2)
        {
            return value1 == value2;
        }

        public override void WriteTo(BinaryWriter writer, TCollection collection)
        {
            writer.Write(collection.Count);
            foreach (var value in collection)
            {
                _typeSerializer.WriteTo(writer, value);
            }
        }

        public override TCollection ReadFrom(BinaryReader reader)
        {
            var collection = new TCollection();
            var count = reader.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var value = _typeSerializer.ReadFrom(reader);
                collection.Add(value);
            }
            return collection;
        }
    }
}