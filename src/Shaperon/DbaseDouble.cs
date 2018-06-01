using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseDouble : DbaseFieldValue
    {
        private static readonly NumberFormatInfo DoubleNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        public DbaseDouble(DbaseField field, double? value = null) : base(field)
        {
            Value = value;
        }

        public double? Value { get; set; }

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
                if (double.TryParse(unpadded, NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, DoubleNumberFormat, out var parsed))
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
                        new string('#', Field.Length - Field.DecimalCount - 1),
                        ".",
                        new string('0', Field.DecimalCount)
                      )
                    : new string('#', Field.Length - Field.DecimalCount - 1);
                var unpadded = Value.Value.ToString(format, DoubleNumberFormat);
                writer.WriteLeftPaddedString(unpadded, Field.Length, ' ');
            }
            else
            {
                writer.Write(new string(' ', Field.Length));
            }
        }

        public override void Inspect(IDbaseFieldValueInspector writer)
        {
            writer.Inspect(this);
        }
    }
}
