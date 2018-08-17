using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseDouble : DbaseFieldValue
    {
        private static readonly NumberFormatInfo DoubleNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
        private const int MaximumDoublePrecision = 15;

        private double? _value;

        public DbaseDouble(DbaseField field, double? value = null) : base(field)
        {
            if (field.FieldType != DbaseFieldType.Number)
            {
                throw new ArgumentException($"The field {field.Name} 's type must be number to use it as a double field.", nameof(field));
            }

            Value = value;
        }

        public double? Value
        {
            get => _value;
            set {
                if(value.HasValue)
                {
                    // if(Double.IsNaN(value.Value))
                    // {
                    //     throw new ArgumentException($"The value of field {Field.Name} can not be not-a-number (NaN).");
                    // }

                    // if(Double.IsNegativeInfinity(value.Value))
                    // {
                    //     throw new ArgumentException($"The value of field {Field.Name} can not be negative infinite.");
                    // }

                    // if(Double.IsPositiveInfinity(value.Value))
                    // {
                    //     throw new ArgumentException($"The value of field {Field.Name} can not be positive infinite.");
                    // }

                    if(Field.DecimalCount == 0)
                    {
                        var truncated = Math.Truncate(value.Value);
                        var length = truncated.ToString("0", DoubleNumberFormat).Length;
                        if(length > Field.Length)
                        {
                            throw new ArgumentException($"The length ({length}) of the value ({truncated}) of field {Field.Name} is greater than its field length {Field.Length}, which would result in loss of precision.");
                        }
                        _value = truncated;
                    }
                    else
                    {
                        var digits = Math.Min(Field.DecimalCount.ToInt32(), MaximumDoublePrecision);
                        var rounded = Math.Round(value.Value, digits);
                        var length = rounded.ToString("0.0##############", DoubleNumberFormat).Length;
                        if(length > Field.Length)
                        {
                            throw new ArgumentException($"The length ({length}) of the value ({rounded}) of field {Field.Name} is greater than its field length {Field.Length}, which would result in loss of precision.");
                        }
                        _value = rounded;
                    }
                }
                else
                {
                    _value = value;
                }
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
                //When decimal count > 0, the format is 0.0#--#
                //where 0 before the decimal separator appears once
                //      . appears once
                //      0 after the decimal separator appears once
                //      # appears 14 times (max precision is 15 digits for doubles)

                //When decimal count = 0, the format is #--#
                //where # appears length times
                var format =
                    Field.DecimalCount > 0
                    ? "0.0##############"
                    : new string('#', Field.Length);
                var unpadded = Value.Value.ToString(format, DoubleNumberFormat);
                if(unpadded.Length < Field.Length && Field.DecimalCount > 0)
                {
                    //When space left after formatting and decimal count > 0,
                    //attempt the format 0.0--0
                    //
                    //where 0 before the decimal separator appears once
                    //      . appears once
                    //      0 after the decimal separator appears decimal count times
                    var altformat = "0." + new string('0', Field.DecimalCount.ToInt32());
                    var altunpadded = Value.Value.ToString(altformat, DoubleNumberFormat);
                    if(altunpadded.Length <= Field.Length)
                    {
                        unpadded = altunpadded;
                    }
                }
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
