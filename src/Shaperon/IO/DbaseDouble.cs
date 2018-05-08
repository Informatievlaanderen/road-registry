using System;
using System.Globalization;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseDouble : DbaseValue
    {
        private static readonly NumberFormatInfo DoubleNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
        
        public DbaseDouble(DbaseField field) : this(field, null)
        {
        }

        public DbaseDouble(DbaseField field, double? value) : base(field)
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
                double parsed;
                if (double.TryParse(unpadded, NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, DoubleNumberFormat, out parsed))
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

        public override void Inspect(IDbaseValueInspector writer)
        {
            writer.Inspect(this);
        }
    }
}