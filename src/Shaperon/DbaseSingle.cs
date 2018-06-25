using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseSingle : DbaseFieldValue
    {
        private static readonly NumberFormatInfo SingleNumberFormat = new NumberFormatInfo { NumberDecimalSeparator = "." };
        private const int MaximumSinglePrecision = 7;

        private float? _value;

        public DbaseSingle(DbaseField field, float? value = null) : base(field)
        {
            if (field.FieldType != DbaseFieldType.Float)
            {
                throw new ArgumentException($"The field {field.Name} 's type must be float to use it as a single field.", nameof(field));
            }

            Value = value;
        }

        public float? Value
        {
            get => _value;
            set {
                if(value.HasValue)
                {
                    // if(Single.IsNaN(value.Value))
                    // {
                    //     throw new ArgumentException($"The value of field {Field.Name} can not be not-a-number (NaN).");
                    // }

                    // if(Single.IsNegativeInfinity(value.Value))
                    // {
                    //     throw new ArgumentException($"The value of field {Field.Name} can not be negative infinite.");
                    // }

                    // if(Single.IsPositiveInfinity(value.Value))
                    // {
                    //     throw new ArgumentException($"The value of field {Field.Name} can not be positive infinite.");
                    // }

                    if(Field.DecimalCount == 0)
                    {
                        var truncated = (float)Math.Truncate(value.Value);
                        var length = truncated.ToString("0", SingleNumberFormat).Length;
                        if(length > Field.Length)
                        {
                            throw new ArgumentException($"The length ({length}) of the value ({truncated}) of field {Field.Name} is greater than its field length {Field.Length}, which would result in loss of precision.");
                        }
                        _value = truncated;
                    }
                    else
                    {
                        var digits = Math.Min(Field.DecimalCount.ToInt32(), MaximumSinglePrecision);
                        var rounded = (float)Math.Round(value.Value, digits);
                        var length = rounded.ToString("0.0######", SingleNumberFormat).Length;
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
                if (float.TryParse(unpadded, NumberStyles.Float | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, SingleNumberFormat, out var parsed))
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
                //      # appears 6 times (max precision is 7 digits for floats)

                //When decimal count = 0, the format is #--#
                //where # appears length times

                var format =
                    Field.DecimalCount > 0
                    ? "0.0######"
                    : new string('#', Field.Length);
                var unpadded = Value.Value.ToString(format, SingleNumberFormat);
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
            return obj is DbaseSingle record
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
