using System;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseFileHeader
    {
        private const byte Terminator = 0x0d;
        private const int HeaderMetaDataSize = 33;
        private const int FieldMetaDataSize = 32;

        public DbaseFileHeader(DateTime lastUpdated, int recordCount, int recordLength, DbaseField[] recordFields)
        {
            LastUpdated = lastUpdated;
            RecordCount = recordCount;
            RecordLength = recordLength;
            RecordFields = recordFields;
        }

        public DateTime LastUpdated { get; }
        public int RecordCount { get; }
        public int RecordLength { get; }
        public DbaseField[] RecordFields { get; }

        public static DbaseFileHeader Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new System.ArgumentNullException(nameof(reader));
            }

            if (reader.ReadByte() != 0x3)
            {
                throw new DbaseFileException("The database file type must be 3 (dBase III).");
            }
            var lastUpdated = new DateTime(reader.ReadByte() + 1900, reader.ReadByte(), reader.ReadByte(), 0, 0, 0, DateTimeKind.Unspecified);
            var recordCount = reader.ReadInt32();
            var headerLength = reader.ReadInt16();
            var recordLength = reader.ReadInt16();
            reader.ReadBytes(20);
            var recordFieldCount = (headerLength - HeaderMetaDataSize) / FieldMetaDataSize;
            var recordFields = new DbaseField[recordFieldCount];
            for (var recordFieldIndex = 0; recordFieldIndex < recordFieldCount; recordFieldIndex++)
            {
                recordFields[recordFieldIndex] = DbaseField.Read(reader);
            }
            if(reader.ReadByte() != Terminator)
            {
                throw new DbaseFileException("The database file header terminator is missing.");
            }
            // skip to first record
            var bytesToSkip = headerLength - (HeaderMetaDataSize + (FieldMetaDataSize * recordFieldCount));
            reader.ReadBytes(bytesToSkip);

            return new DbaseFileHeader(lastUpdated, recordCount, recordLength, recordFields);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.Write(Convert.ToByte(3));
            writer.Write(Convert.ToByte(LastUpdated.Year - 1900));
            writer.Write(Convert.ToByte(LastUpdated.Month));
            writer.Write(Convert.ToByte(LastUpdated.Day));
            writer.Write(RecordCount);
            var headerLength = HeaderMetaDataSize + (FieldMetaDataSize * RecordFields.Length);
            writer.Write(Convert.ToInt16(headerLength));
            writer.Write(Convert.ToInt16(RecordLength));
            writer.Write(new byte[20]);
            foreach(var recordField in RecordFields)
            {
                recordField.Write(writer);
            }
            writer.Write(Terminator);
        }
    }
}