using System;
using System.Globalization;
using System.IO;

namespace Shaperon
{
    public class DbaseDateTime : DbaseValue
    {
        public DbaseDateTime(DbaseField field) : this(field, null)
        {
        }

        public DbaseDateTime(DbaseField field, DateTime? value) : base(field)
        {
            Value = value;
        }

        public DateTime? Value { get; set; }

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
                var unpadded = reader.ReadRightPaddedString(Field.Length, ' ');
                if (DateTime.TryParseExact(unpadded, "yyyyMMdd\\THHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite, out DateTime parsed))
                {
                    Value = new DateTime(parsed.Year, parsed.Month, parsed.Day, parsed.Hour, parsed.Minute, parsed.Second, DateTimeKind.Unspecified);
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
                var unpadded = Value.Value.ToString("yyyyMMddTHHmmss", CultureInfo.InvariantCulture);
                writer.WriteRightPaddedString(unpadded, Field.Length, ' ');
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