using System;
using System.IO;

namespace Shaperon
{
    public class DbaseFileHeader
    {
        public const int MaximumFileSize = 1073741824; // 1 GB
        public const byte Terminator = 0x0d;
        public const byte ExpectedDbaseFormat = 0x03;
        public const int HeaderMetaDataSize = 33;
        public const int FieldMetaDataSize = 32;

        public DbaseFileHeader(DateTime lastUpdated, DbaseCodePage codePage, int recordCount, int recordLength, DbaseField[] recordFields)
        {
            LastUpdated = lastUpdated.RoundToDay();
            CodePage = codePage;
            RecordCount = recordCount;
            RecordLength = recordLength;
            RecordFields = recordFields;
        }

        public DbaseFileHeader(DateTime lastUpdated, DbaseCodePage codePage, DbaseRecordCount recordCount, DbaseSchema schema)
        {
            LastUpdated = lastUpdated.RoundToDay();
            CodePage = codePage;
            RecordCount = recordCount;
            Schema = schema;
        }

        public DateTime LastUpdated { get; }
        public DbaseCodePage CodePage { get; }
        public int RecordCount { get; }
        public DbaseSchema Schema { get; }
        [Obsolete]
        public int RecordLength { get; }
        [Obsolete]
        public DbaseField[] RecordFields { get; }

        public static DbaseFileHeader Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (reader.ReadByte() != ExpectedDbaseFormat)
            {
                throw new DbaseFileHeaderException("The database file type must be 3 (dBase III).");
            }
            var lastUpdated = new DateTime(reader.ReadByte() + 1900, reader.ReadByte(), reader.ReadByte(), 0, 0, 0, DateTimeKind.Unspecified);
            var recordCount = reader.ReadInt32();
            var headerLength = reader.ReadInt16();
            var recordLength = reader.ReadInt16();
            reader.ReadBytes(16);
            var rawCodePage = reader.ReadByte();
            if(!DbaseCodePage.TryParse(rawCodePage, out DbaseCodePage codePage))
            {
                throw new DbaseFileHeaderException($"The database code page {rawCodePage} is not supported.");
            }
            reader.ReadBytes(3);
            var fieldCount = (headerLength - HeaderMetaDataSize) / FieldMetaDataSize;
            var fields = new DbaseField[fieldCount];
            for (var recordFieldIndex = 0; recordFieldIndex < fieldCount; recordFieldIndex++)
            {
                fields[recordFieldIndex] = DbaseField.Read(reader);
            }
            if(reader.ReadByte() != Terminator)
            {
                throw new DbaseFileHeaderException("The database file header terminator is missing.");
            }
            // skip to first record
            var bytesToSkip = headerLength - (HeaderMetaDataSize + (FieldMetaDataSize * fieldCount));
            reader.ReadBytes(bytesToSkip);

            var schema = new AnonymousDbaseSchema(fields);

            return new DbaseFileHeader(lastUpdated, codePage, new DbaseRecordCount(recordCount), schema);
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
            writer.Write(new byte[16]);
            writer.Write(CodePage.ToByte());
            writer.Write(new byte[3]);
            foreach(var recordField in RecordFields)
            {
                recordField.Write(writer);
            }
            writer.Write(Terminator);
        }
    }
}
