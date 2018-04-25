using System;
using System.IO;

namespace Shaperon.IO
{
    public class DbaseFileHeader
    {
        public DbaseFileHeader(DateTime lastUpdated, int recordCount, int recordLength, DbaseRecordField[] recordFields)
        {
            LastUpdated = lastUpdated;
            RecordCount = recordCount;
            RecordLength = recordLength;
            RecordFields = recordFields;
        }

        public DateTime LastUpdated { get; }
        public int RecordCount { get; }
        public int RecordLength { get; }
        public DbaseRecordField[] RecordFields { get; }

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
            var recordFieldCount = (headerLength - 33) / 32;
            var recordFields = new DbaseRecordField[recordFieldCount];
            for (var recordFieldIndex = 0; recordFieldIndex < recordFieldCount; recordFieldIndex++)
            {
                recordFields[recordFieldIndex] = DbaseRecordField.Read(reader);
            }
            if(reader.ReadByte() != 0x0d)
            {
                throw new DbaseFileException("The database file header terminator is missing.");
            }
            return new DbaseFileHeader(lastUpdated, recordCount, recordLength, recordFields);
        }
    }
}