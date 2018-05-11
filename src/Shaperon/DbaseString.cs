using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseString : DbaseValue
    {
        private string _value;

        public DbaseString(DbaseField field) : this(field, null)
        {
        }

        public DbaseString(DbaseField field, string value) : base(field)
        {
            Value = value;
        }

        public string Value 
        { 
            get => _value;
            set 
            {
                if(_value != null && _value.Length > Field.Length)
                {
                    throw new ArgumentException($"The value length {_value.Length} is greater than the field length {Field.Length}.");
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

            if(Value != null)
            {
                writer.WriteRightPaddedString(Value, Field.Length, ' ');
            }
            else
            {
                writer.Write(new string(' ', Field.Length));
            }
        }

        public DbaseValue TryInferDateTime()
        {
            if(Value != null && Field.Length == 15 && DateTime.TryParseExact(Value, "yyyyMMdd\\THHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite, out DateTime parsed))
            {
                return new DbaseDateTime(Field, new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, parsed.Minute, parsed.Second, DateTimeKind.Unspecified));
            }
            return this;
        }

        public override void Inspect(IDbaseValueInspector writer)
        {
            writer.Inspect(this);
        }
    }
}