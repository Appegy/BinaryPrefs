using System.Collections.Generic;
using System.IO;

namespace Appegy.BinaryStorage
{
    public class KeyValueTypeSerializer<TKey, TValue> : TypeSerializer<KeyValuePair<TKey, TValue>>
    {
        private readonly TypeSerializer<TKey> _keySerializer;
        private readonly TypeSerializer<TValue> _valueSerializer;

        public override string TypeName { get; }

        public KeyValueTypeSerializer(TypeSerializer<TKey> keySerializer, TypeSerializer<TValue> valueSerializer)
        {
            _keySerializer = keySerializer;
            _valueSerializer = valueSerializer;
            TypeName = $"{typeof(TKey).Name}:{typeof(TValue).Name}";
        }

        public override int SizeOf(KeyValuePair<TKey, TValue> value)
        {
            return _keySerializer.SizeOf(value.Key) + _valueSerializer.SizeOf(value.Value);
        }

        public override bool Equals(KeyValuePair<TKey, TValue> value1, KeyValuePair<TKey, TValue> value2)
        {
            return _keySerializer.Equals(value1.Key, value2.Key) && _valueSerializer.Equals(value1.Value, value2.Value);
        }

        public override void WriteTo(BinaryWriter writer, KeyValuePair<TKey, TValue> value)
        {
            _keySerializer.WriteTo(writer, value.Key);
            _valueSerializer.WriteTo(writer, value.Value);
        }

        public override KeyValuePair<TKey, TValue> ReadFrom(BinaryReader reader)
        {
            return new KeyValuePair<TKey, TValue>(_keySerializer.ReadFrom(reader), _valueSerializer.ReadFrom(reader));
        }
    }
}