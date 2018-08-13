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
                    DbaseFieldType.Float,
                    field => {
                        if(field.DecimalCount == 0)
                        {
                            return new DbaseInt32(field);
                        }
                        return new DbaseSingle(field);
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
            switch(fieldType)
            {
                case DbaseFieldType.Character:
                    if(decimalCount != 0)
                    {
                        throw new ArgumentException($"The character field {name} decimal count ({decimalCount}) must be set to 0.", nameof(decimalCount));
                    }
                    break;
                case DbaseFieldType.DateTime:
                    if(length != 15)
                    {
                        throw new ArgumentException($"The datetime field {name} length ({length}) must be set to 15.");
                    }
                    if(decimalCount != 0)
                    {
                        throw new ArgumentException($"The datetime field {name} decimal count ({decimalCount}) must be set to 0.", nameof(decimalCount));
                    }
                    break;
                case DbaseFieldType.Number:
                    if(decimalCount != 0 && decimalCount > length - 2)
                    {
                        throw new ArgumentException($"The number field {name} decimal count ({decimalCount}) must be 2 less than its length ({length}).");
                    }
                    break;
                case DbaseFieldType.Float:
                    if(decimalCount != 0 && decimalCount > length - 2)
                    {
                        throw new ArgumentException($"The float field {name} decimal count ({decimalCount}) must be 2 less than its length ({length}).");
                    }
                    break;
            }
            Name = name;
            FieldType = fieldType;
            Offset = offset;
            Length = length;
            DecimalCount = decimalCount;
        }

        public DbaseField After(DbaseField field)
        {
            if (field == null)
            {
                throw new ArgumentNullException(nameof(field));
            }

            return new DbaseField(Name, FieldType, field.Offset.Plus(field.Length.ToByteLength()), Length, DecimalCount);
        }

        public static DbaseField CreateStringField(DbaseFieldName name, DbaseFieldLength length)
        {
            return new DbaseField(name, DbaseFieldType.Character, new ByteOffset(0), length, new DbaseDecimalCount(0));
        }

        public static DbaseField CreateInt32Field(DbaseFieldName name, DbaseFieldLength length)
        {
            return new DbaseField(name, DbaseFieldType.Number, new ByteOffset(0), length, new DbaseDecimalCount(0));
        }

        public static DbaseField CreateDateTimeField(DbaseFieldName name)
        {
            return new DbaseField(name, DbaseFieldType.DateTime, new ByteOffset(0), new DbaseFieldLength(15), new DbaseDecimalCount(0));
        }

        public static DbaseField CreateDoubleField(DbaseFieldName name, DbaseFieldLength length, DbaseDecimalCount decimalCount)
        {
            return new DbaseField(name, DbaseFieldType.Number, new ByteOffset(0), length, decimalCount);
        }

        public static DbaseField CreateSingleField(DbaseFieldName name, DbaseFieldLength length, DbaseDecimalCount decimalCount)
        {
            return new DbaseField(name, DbaseFieldType.Float, new ByteOffset(0), length, decimalCount);
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
                throw new DbaseFileHeaderException($"The field type {typeOfField} of field {name} is not supported.");
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
