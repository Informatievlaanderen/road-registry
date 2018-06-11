using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseInt32 : DbaseFieldValue
    {
        public DbaseInt32(DbaseField field, int? value = null) : base(field)
        {
            if (field.FieldType != DbaseFieldType.Number)
            {
                throw new ArgumentException($"The field {field.Name} 's type must be number to use it as an integer field.", nameof(field));
            }

            if (field.DecimalCount.ToInt32() != 0)
            {
                throw new ArgumentException($"The number field {field.Name} 's decimal count must be 0 to use it as an integer field.", nameof(field));
            }

            Value = value;
        }

        public int? Value { get; set; }

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
                var unpadded = Value.Value.ToString(CultureInfo.InvariantCulture);
                writer.WriteLeftPaddedString(unpadded, Field.Length, ' ');
            }
            else
            {
                writer.Write(new string(' ', Field.Length).ToCharArray());
            }
        }

        public override void Inspect(IDbaseFieldValueInspector writer)
        {
            writer.Inspect(this);
        }
    }
}
