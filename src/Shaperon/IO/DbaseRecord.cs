using System;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseRecord
    {
        public const byte EndOfFile = 0x1a;

        private DbaseRecord(bool deleted, DbaseValue[] values)
        {
            IsDeleted = deleted;
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public bool IsDeleted { get; }
        public DbaseValue[] Values { get; }

        public static DbaseRecord Create(DbaseValue[] values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return new DbaseRecord(false, values);
        }

        public static DbaseRecord Read(BinaryReader reader, DbaseFileHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            var flag = reader.ReadByte();
            if(flag == EndOfFile)
            {
                throw new DbaseFileException("The end of file was reached unexpectedly.");
            }
            if(flag != 0x20 && flag != 0x2A) 
            {
                throw new DbaseFileException($"The record deleted flag must be either deleted (0x2A) or valid (0x20) but is 0x{flag:X2}");
            }
            var deleted = flag == 0x2A;
            var values = new DbaseValue[header.RecordFields.Length];
            for(var index = 0; index < header.RecordFields.Length; index++)
            {
                var value = header.RecordFields[index].CreateValue();
                value.Read(reader);
                if(value is DbaseString)
                {
                    value = ((DbaseString)value).TryInferDateTime();
                }
                values[index] = value;
            }
            return new DbaseRecord(deleted, values);
        }
        
        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            writer.Write(Convert.ToByte(IsDeleted ? 0x2A : 0x20));
            foreach(var value in Values)
            {
                value.Write(writer);
            }
        }

        public byte[] ToBytes()
        {
            using(var output = new MemoryStream())
            {
                using(var writer = new BinaryWriter(output))
                {
                    Write(writer);
                    writer.Flush();
                }
                return output.ToArray();
            }
        }
    }
}