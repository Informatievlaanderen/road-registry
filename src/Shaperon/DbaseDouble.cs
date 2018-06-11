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
            if (field.FieldType != DbaseFieldType.Number)
            {
                throw new ArgumentException($"The field {field.Name} 's type must be number to use it as a double field.", nameof(field));
            }

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
                //With decimal count > 0 the format is #--#0.0--0
                //where # appears length - 2 - decimalcount times
                //      0 before the decimal separator appears once
                //      . appears once
                //      0 appears decimal count times

                //With decimal count = 0 the format is #--#
                //where # appears length times

                var format = Field.DecimalCount > 0
                    ? String.Concat(
                        new string('#', Field.Length - Field.DecimalCount - 2),
                        "0.",
                        new string('0', Field.DecimalCount)
                      )
                    : new string('#', Field.Length);
                var unpadded = Value.Value.ToString(format, DoubleNumberFormat);
                if(unpadded.Length > Field.Length)
                {
                    // TODO: We may want to log we're loosing some of our precision here (and log exactly how much we're loosing). Is this acceptable?
                    unpadded = unpadded.Substring(0, Field.Length);
                }
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
