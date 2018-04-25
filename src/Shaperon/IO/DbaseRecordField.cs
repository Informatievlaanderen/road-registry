using System;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseRecordField
    {
        public DbaseRecordField(string name, RecordFieldType fieldType, int length, int decimalCount)
        {
            Name = name;
            FieldType = fieldType;
            Length = length;
            DecimalCount = decimalCount;
        }

        public string Name { get; }
        public RecordFieldType FieldType { get; }
        public int Length { get; }
        public int DecimalCount { get; }

        public static DbaseRecordField Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var name = reader.ReadRightPaddedString(11, char.MinValue);
            var typeOfField = reader.ReadByte();
            if(!(Enum.IsDefined(typeof(RecordFieldType), typeOfField)))
            {
                throw new DbaseFileException($"The field type {typeOfField} of field {name} is not supported.");
            }
            reader.ReadInt32();
            var length = reader.ReadByte();
            var decimals = reader.ReadByte();
            reader.ReadBytes(14);
            return new DbaseRecordField(name, (RecordFieldType)typeOfField, length, decimals);
        }
    }
}