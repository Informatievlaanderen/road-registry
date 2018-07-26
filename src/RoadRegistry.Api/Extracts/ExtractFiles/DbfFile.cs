namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Collections.Generic;
    using System.Text;
    using Shaperon;

    public class DbfFile<TDbaseRecord> : ExtractFile
        where TDbaseRecord : DbaseRecord
    {
        private static Encoding Encoding => Encoding.GetEncoding(1252);


        public DbfFile(string name, DbaseFileHeader header)
            : base(name, ".dbf", Encoding)
        {
            header.Write(Writer);
        }

        public void Write(TDbaseRecord record)
        {
            record.Write(Writer);
        }

        public void Write(IEnumerable<TDbaseRecord> records)
        {
            foreach (var record in records)
            {
                Write(record);
            }
        }

        public void WriteBytesAs<T>(byte[] record)
           where T : TDbaseRecord, new()
        {
            var dbaseRecord = new T();
            dbaseRecord.FromBytes(record, Encoding);
            Write(dbaseRecord);
        }

        public void WriteBytesAs<T>(IEnumerable<byte[]> records)
            where T : TDbaseRecord, new()
        {
            foreach (var record in records)
            {
                WriteBytesAs<T>(record);
            }
        }

        protected sealed override void BeforeFlush()
        {
            Writer.Write(DbaseRecord.EndOfFile);
        }
    }
}
