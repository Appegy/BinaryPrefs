using System;

namespace Appegy.BinaryStorage
{
    internal abstract class ValueRecord
    {
        public abstract Type Type { get; }
    }

    internal class ValueRecord<T> : ValueRecord
    {
        private readonly TypeSerializer<T> _serializer;
        private T _value;

        public override Type Type { get; }

        public ValueRecord(TypeSerializer<T> serializer, T value)
        {
            Type = typeof(T);
            _serializer = serializer;
            _value = value;
        }

        public T GetValue() => _value;

        public bool SetValue(T value)
        {
            if (_serializer.Equals(_value, value))
            {
                return false;
            }
            _value = value;
            return true;
        }
    }
}