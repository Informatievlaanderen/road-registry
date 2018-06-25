using System;
using System.IO;
using System.Linq;

namespace Shaperon
{
    public abstract class DbaseRecord
    {
        public const byte EndOfFile = 0x1a;

        protected DbaseRecord()
        {
        }

        public bool IsDeleted { get; set; }

        public DbaseFieldValue[] Values { get; protected set; }

        public void Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var flag = reader.ReadByte();
            if(flag == EndOfFile)
            {
                throw new DbaseFileHeaderException("The end of file was reached unexpectedly.");
            }
            if(flag != 0x20 && flag != 0x2A)
            {
                throw new DbaseFileHeaderException($"The record deleted flag must be either deleted (0x2A) or valid (0x20) but is 0x{flag:X2}");
            }
            IsDeleted = flag == 0x2A;
            for(var index = 0; index < Values.Length; index++)
            {
                var value = Values[index];
                value.Read(reader);
                if(value is DbaseString candidate)
                {
                    Values[index] = candidate.TryInferDateTime();
                }
            }
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

        public override bool Equals(object obj)
        {
            return obj is DbaseRecord record
                   && GetType() == obj.GetType()
                   && Equals(record);
        }

        private bool Equals(DbaseRecord other)
        {
            return IsDeleted == other.IsDeleted && EqualValues(other);
        }

        private bool EqualValues(DbaseRecord other)
        {
            if (null == Values && null == other.Values)
                return true;

            if (Values?.Length != other.Values?.Length)
                return false;

            return Values
                .Select<DbaseFieldValue, Func<bool>>((value, i) => { return () => value.Equals(other.Values[i]); })
                .All(areEqual => areEqual());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (IsDeleted.GetHashCode() * 397) ^ (Values != null ? Values.GetHashCode() : 0);
            }
        }
    }
}
