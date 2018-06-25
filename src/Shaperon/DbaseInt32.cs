using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseInt32 : DbaseFieldValue
    {
        private int? _value;

        public DbaseInt32(DbaseField field, int? value = null) : base(field)
        {
            if (field.FieldType != DbaseFieldType.Number && field.FieldType != DbaseFieldType.Float)
            {
                throw new ArgumentException($"The field {field.Name}'s type must be either number or float to use it as an integer field.", nameof(field));
            }

            if (field.DecimalCount != 0)
            {
                throw new ArgumentException($"The number field {field.Name}'s decimal count must be 0 to use it as an integer field.", nameof(field));
            }

            Value = value;
        }

        public int? Value
        {
            get => _value;
            set {
                if(value.HasValue)
                {
                    var length = FormatAsString(value.Value).Length;
                    if (length > Field.Length)
                    {
                        throw new ArgumentException($"The value length {length} of field {Field.Name} is greater than its field length {Field.Length}.");
                    }
                }
                _value = value;
            }
        }

        private static string FormatAsString(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        public override void Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.PeekChar() == '\0')
            {
                reader.ReadBytes(Field.Length);
                Value = null;
            }
            else
            {
                var unpadded = reader.ReadLeftPaddedString(Field.Length, ' ');
                if (int.TryParse(unpadded, NumberStyles.Integer | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out var parsed))
                {
                    Value = parsed;
                }
                else
                {
                    Value = null;
                }
            }
        }

        public override void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if(Value.HasValue)
            {
                var unpadded = FormatAsString(Value.Value);
                writer.WriteLeftPaddedString(unpadded, Field.Length, ' ');
            }
            else
            {
                writer.Write(new string(' ', Field.Length).ToCharArray());
                // or writer.Write(new byte[Field.Length]); // to determine
            }
        }

        public override void Inspect(IDbaseFieldValueInspector writer)
        {
            writer.Inspect(this);
        }

        public override bool Equals(object obj)
        {
            return obj is DbaseInt32 record
                   && base.Equals(record)
                   && _value.Equals(record._value);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ _value.GetHashCode();
            }
        }
    }
}
