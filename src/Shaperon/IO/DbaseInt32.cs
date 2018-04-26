using System;
using System.Globalization;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseInt32 : DbaseValue
    {
        private static readonly NumberFormatInfo Int32NumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
        public DbaseInt32(DbaseField field) : this(field, null)
        {
        }

        public DbaseInt32(DbaseField field, int? value) : base(field)
        {
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
                int parsed;
                if (int.TryParse(unpadded, NumberStyles.Integer | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, Int32NumberFormat, out parsed))
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
                var format = Field.DecimalCount > 0 
                    ? String.Concat(
                        new string('0', Field.Length - Field.DecimalCount - 1),
                        ".",
                        new string('0', Field.DecimalCount)
                      )
                    : new string('0', Field.Length - Field.DecimalCount - 1);
                var unpadded = Value.Value.ToString(format, Int32NumberFormat);
                writer.WriteLeftPaddedString(unpadded, Field.Length, ' ');
            }
            else
            {
                writer.Write(new string(' ', Field.Length));
            }
        }

        public override void Inspect(IDbaseValueInspector writer)
        {
            writer.Inspect(this);
        }
    }
}