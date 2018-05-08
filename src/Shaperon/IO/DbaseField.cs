using System;
using System.Collections.Generic;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseField
    {
        private readonly Dictionary<DbaseFieldType, Func<DbaseField, DbaseValue>> _factories =
            new Dictionary<DbaseFieldType, Func<DbaseField, DbaseValue>> 
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
        
        public DbaseValue CreateValue()
        {
            if(!_factories.TryGetValue(FieldType, out Func<DbaseField, DbaseValue> factory))
            {
                throw new NotSupportedException($"The field type {FieldType} of field {Name} is not supported.");
            }
            return factory(this);
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