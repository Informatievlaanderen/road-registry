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
        public DbaseField(string name, DbaseFieldType fieldType, int length, int decimalCount)
        {
            Name = name;
            FieldType = fieldType;
            Length = length;
            DecimalCount = decimalCount;
        }

        public string Name { get; }
        public DbaseFieldType FieldType { get; }
        public int Length { get; }
        public int DecimalCount { get; }
        
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

            var name = reader.ReadRightPaddedString(11, char.MinValue);
            var typeOfField = reader.ReadByte();
            if(!(Enum.IsDefined(typeof(DbaseFieldType), typeOfField)))
            {
                throw new DbaseFileException($"The field type {typeOfField} of field {name} is not supported.");
            }
            var fieldType = (DbaseFieldType)typeOfField;
            // if(fieldType != DbaseFieldType.Number || fieldType != DbaseFieldType.Character || fieldType != DbaseFieldType.DateTime)
            // {
            //     throw new DbaseFileException($"The field type {fieldType} of field {name} is not supported.");
            // }
            reader.ReadInt32();
            var length = reader.ReadByte();
            var decimals = reader.ReadByte();
            reader.ReadBytes(14);
            return new DbaseField(name, (DbaseFieldType)typeOfField, length, decimals);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteRightPaddedString(Name, 11, char.MinValue);
            // HACK: Because legacy represents date times as characters - so why bother with DateTime support?
            if(FieldType == DbaseFieldType.DateTime)
            {
                writer.Write((byte)DbaseFieldType.Character);
            }
            else
            {
                writer.Write((byte)FieldType);
            }
            writer.Write(0); //TODO: Figure out what we need to fill in here.
            writer.Write(Convert.ToByte(Length));
            writer.Write(Convert.ToByte(DecimalCount));
            writer.Write(new byte[14]);
        }
    }
}