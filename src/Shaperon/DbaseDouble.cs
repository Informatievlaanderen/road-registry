using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseDouble : DbaseFieldValue
    {
        private static readonly NumberFormatInfo DoubleNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };

        private double? _value;

        public DbaseDouble(DbaseField field, double? value = null) : base(field)
        {
            if (field.FieldType != DbaseFieldType.Number)
            {
                throw new ArgumentException($"The field {field.Name} 's type must be number to use it as a double field.", nameof(field));
            }

            //With decimal count > 0 the format is #--#0.0--0
            //where # appears length - 2 - decimalcount times
            //      0 before the decimal separator appears once
            //      . appears once
            //      0 appears decimal count times

            //With decimal count = 0 the format is #--#
            //where # appears length times

            Format =
                Field.DecimalCount > 0
                ? String.Concat(
                    new string('#', Field.Length - Field.DecimalCount - 2),
                    "0.",
                    new string('0', Field.DecimalCount)
                    )
                : new string('#', Field.Length);
            Value = value;
        }

        private string Format { get; }

        public double? Value
        {
            get => _value;
            set {
                if(value.HasValue)
                {
                    var length = FormatAsString(value.Value).Length;
                    if(length > Field.Length)
                    {
                        throw new ArgumentException($"The length {length} of value {value.Value} using format {Format} of field {Field.Name} is greater than its field length {Field.Length}.");
                    }
                }
                _value = value;
            }
        }

        private string FormatAsString(double value)
        {
            return value.ToString(Format, DoubleNumberFormat);
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
    }
}
