using System;
using System.IO;
using Shaperon;

namespace RoadRegistry.Projections
{
    public class RoadNodeDbaseRecord
    {
        public static readonly RoadNodeDbaseSchema Schema = new RoadNodeDbaseSchema();

        public RoadNodeDbaseRecord()
        {
            WK_OIDN = new DbaseInt32(Schema.WK_OIDN);
            WK_UIDN = new DbaseString(Schema.WK_UIDN);
            TYPE = new DbaseInt32(Schema.WK_UIDN);
            LBLTYPE = new DbaseString(Schema.WK_UIDN);
            BEGINTIJD = new DbaseDateTime(Schema.WK_UIDN);
            BEGINORG = new DbaseString(Schema.WK_UIDN);
            LBLBGNORG = new DbaseString(Schema.WK_UIDN);
            IsDeleted = false;
        }

        public DbaseInt32 WK_OIDN { get; }
        public DbaseString WK_UIDN { get; }
        public DbaseInt32 TYPE { get; }
        public DbaseString LBLTYPE { get; }
        public DbaseDateTime BEGINTIJD { get; }
        public DbaseString BEGINORG { get; }
        public DbaseString LBLBGNORG { get; }
        public bool IsDeleted { get; set; }

        public void Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var flag = reader.ReadByte();
            if(flag == DbaseRecord.EndOfFile)
            {
                throw new DbaseFileException("The end of file was reached unexpectedly.");
            }
            if(flag != 0x20 && flag != 0x2A)
            {
                throw new DbaseFileException($"The record deleted flag must be either deleted (0x2A) or valid (0x20) but is 0x{flag:X2}");
            }
            IsDeleted = flag == 0x2A;
            WK_OIDN.Read(reader);
            WK_UIDN.Read(reader);
            TYPE.Read(reader);
            LBLTYPE.Read(reader);
            BEGINTIJD.Read(reader);
            BEGINORG.Read(reader);
            LBLBGNORG.Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }
            writer.Write(Convert.ToByte(IsDeleted ? 0x2A : 0x20));
            WK_OIDN.Write(writer);
            WK_UIDN.Write(writer);
            TYPE.Write(writer);
            LBLTYPE.Write(writer);
            BEGINTIJD.Write(writer);
            BEGINORG.Write(writer);
            LBLBGNORG.Write(writer);
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
