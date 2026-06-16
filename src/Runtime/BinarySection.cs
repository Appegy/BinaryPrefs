using System;
using System.IO;

namespace Appegy.Storage
{
    internal abstract class BinarySection
    {
        public int Count { get; set; }
        public TypeSerializer Serializer { get; }

        protected BinarySection(TypeSerializer serializer)
        {
            Serializer = serializer;
        }

        /// <summary> Gets the runtime type handled by this section. </summary>
        public abstract Type Type { get; }

        public abstract string TypeName { get; }
        public abstract Record ReadFrom(BinaryReader binaryReader, int typeIndex);
        public abstract void WriteTo(BinaryWriter binaryWriter, Record record);

        /// <summary> Creates a new record from an untyped value. </summary>
        /// <param name="value">The value to store. Must be of the type handled by this section.</param>
        /// <param name="typeIndex">The index of this section in the supported types list.</param>
        /// <returns>A new record wrapping the value.</returns>
        public abstract Record CreateRecord(object value, int typeIndex);

        /// <summary> Updates the value of an existing record if it differs from the current value. </summary>
        /// <param name="record">The record to update. Must have been created by this section.</param>
        /// <param name="value">The new value. Must be of the type handled by this section.</param>
        /// <returns>True if the value was changed; false if it was equal to the existing value.</returns>
        public abstract bool UpdateRecord(Record record, object value);
    }

    internal class TypedBinarySection<T> : BinarySection
    {
        private readonly TypeSerializer<T> _serializer;

        public new TypeSerializer<T> Serializer => _serializer;
        public override Type Type => typeof(T);
        public override string TypeName => Serializer.TypeName;

        public TypedBinarySection(TypeSerializer<T> serializer)
            : base(serializer)
        {
            _serializer = serializer;
        }

        public override Record ReadFrom(BinaryReader binaryReader, int typeIndex)
        {
            return new Record<T>(_serializer.ReadFrom(binaryReader), typeIndex);
        }

        public override void WriteTo(BinaryWriter binaryWriter, Record record)
        {
            _serializer.WriteTo(binaryWriter, ((Record<T>)record).Value);
        }

        public override Record CreateRecord(object value, int typeIndex)
        {
            return new Record<T>((T)value, typeIndex);
        }

        public override bool UpdateRecord(Record record, object value)
        {
            var typedRecord = (Record<T>)record;
            if (_serializer.Equals(typedRecord.Value, (T)value))
            {
                return false;
            }
            typedRecord.Value = (T)value;
            return true;
        }
    }
}