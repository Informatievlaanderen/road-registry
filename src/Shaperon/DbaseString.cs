using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseString : DbaseFieldValue
    {
        private string _value;

        public DbaseString(DbaseField field, string value = null) : base(field)
        {
            if (field.FieldType != DbaseFieldType.Character)
            {
                throw new ArgumentException($"The field {field.Name} 's type must be character to use it as a string field.", nameof(field));
            }

            Value = value;
        }

        public string Value
        {
            get => _value;
            set
            {
                if(value != null && value.Length > Field.Length)
                {
                    throw new ArgumentException($"The value length {value.Length} of field {Field.Name} is greater than its field length {Field.Length}.");
                }
                _value = value;
            }
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
                Value = reader.ReadRightPaddedString(Field.Length, ' ');
            }
        }

        public override void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if(Value == null)
            {
                writer.Write(new byte[Field.Length]);
            }
            else
            {
                writer.WriteRightPaddedString(Value, Field.Length, ' ');
            }
        }

        public DbaseFieldValue TryInferDateTime()
        {
            if(Value != null && Field.Length == 15 && DateTime.TryParseExact(Value, "yyyyMMdd\\THHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite, out DateTime parsed))
            {
                return new DbaseDateTime(Field, new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, parsed.Minute, parsed.Second, DateTimeKind.Unspecified));
            }
            return this;
        }

        public override void Inspect(IDbaseFieldValueInspector writer)
        {
            writer.Inspect(this);
        }
    }
}
