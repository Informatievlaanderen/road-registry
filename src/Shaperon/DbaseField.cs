using System;
using System.Collections.Generic;
using System.IO;

namespace Shaperon
{
    public class DbaseField
    {
        private readonly Dictionary<DbaseFieldType, Func<DbaseField, DbaseFieldValue>> _factories =
            new Dictionary<DbaseFieldType, Func<DbaseField, DbaseFieldValue>>
            {
                {
                    DbaseFieldType.Character,
                    field => new DbaseString(field)
                },
                {
                    DbaseFieldType.Number,
                    field => {
                        if(field.DecimalCount == 0)
                        {
                            return new DbaseInt32(field);
                        }
                        return new DbaseDouble(field);
                    }
                },
                {
                    DbaseFieldType.DateTime,
                    field => new DbaseDateTime(field)
                }
            };
        public DbaseField(DbaseFieldName name, DbaseFieldType fieldType, ByteOffset offset, DbaseFieldLength length, DbaseDecimalCount decimalCount)
        {
            if(!Enum.IsDefined(typeof(DbaseFieldType), fieldType))
            {
                throw new ArgumentException($"The field type {fieldType} of field {name} is not supported.", nameof(fieldType));
            }
            Name = name;
            FieldType = fieldType;
            Offset = offset;
            Length = length;
            DecimalCount = decimalCount;

            //TODO: Verify the compatibility of the length with the field type.
        }

        public DbaseFieldName Name { get; }
        public DbaseFieldType FieldType { get; }
        public ByteOffset Offset { get; }
        public DbaseFieldLength Length { get; }
        public DbaseDecimalCount DecimalCount { get; }

        private bool Equals(DbaseField other) =>
            other != null &&
            Name == other.Name &&
            // HACK: Because legacy represents date times as characters - so why bother with DateTime support?
            (
                (
                    (FieldType == DbaseFieldType.Character || FieldType == DbaseFieldType.DateTime)
                    &&
                    (other.FieldType == DbaseFieldType.Character || other.FieldType == DbaseFieldType.DateTime)
                )
                ||
                FieldType == other.FieldType
            ) &&
            Offset == other.Offset &&
            Length == other.Length &&
            DecimalCount == other.DecimalCount;

        public override bool Equals(object obj) =>
            obj is DbaseField field && Equals(field);

        public override int GetHashCode() =>
            Name.GetHashCode() ^
            // HACK: Because legacy represents date times as characters - so why bother with DateTime support?
            (FieldType == DbaseFieldType.DateTime ? DbaseFieldType.Character : FieldType).GetHashCode() ^
            Offset.GetHashCode() ^
            Length.GetHashCode() ^
            DecimalCount.GetHashCode();

        public DbaseFieldValue CreateFieldValue()
        {
            return _factories[FieldType](this);
        }

        public static DbaseField Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var name = new DbaseFieldName(reader.ReadRightPaddedString(11, char.MinValue));
            var typeOfField = reader.ReadByte();
            if(!(Enum.IsDefined(typeof(DbaseFieldType), typeOfField)))
            {
                throw new DbaseFileException($"The field type {typeOfField} of field {name} is not supported.");
            }
            var fieldType = (DbaseFieldType)typeOfField;
            var offset = new ByteOffset(reader.ReadInt32());
            var length = new DbaseFieldLength(reader.ReadByte());
            var decimalCount = new DbaseDecimalCount(reader.ReadByte());
            reader.ReadBytes(14);
            return new DbaseField(name, (DbaseFieldType)typeOfField, offset, length, decimalCount);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteRightPaddedString(Name.ToString(), 11, char.MinValue);
            // HACK: Because legacy represents date times as characters - so why bother with DateTime support?
            if(FieldType == DbaseFieldType.DateTime)
            {
                writer.Write((byte)DbaseFieldType.Character);
            }
            else
            {
                writer.Write((byte)FieldType);
            }
            writer.Write(Offset.ToInt32());
            writer.Write(Length.ToByte());
            writer.Write(DecimalCount.ToByte());
            writer.Write(new byte[14]);
        }
    }
}
