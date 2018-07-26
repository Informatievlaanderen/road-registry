namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Collections.Generic;
    using System.Text;
    using Projections;
    using Shaperon;

    public class DbfFile<T> : ExtractFile
        where T : DbaseRecord, new()
    {
        private static Encoding Encoding => Encoding.GetEncoding(1252);

        private readonly T _dbaseRecord;

        public DbfFile(string name, DbaseFileHeader header)
            : base(name, ".dbf", Encoding)
        {
            _dbaseRecord = new T();
            header.Write(Writer);
        }

        public void Write(IBinaryReadableRecord record)
        {
            _dbaseRecord.FromBytes(record.DbaseRecord, Encoding);
            _dbaseRecord.Write(Writer);
        }

        public void Write(IEnumerable<IBinaryReadableRecord> records)
        {
            foreach (var record in records)
            {
                Write(record);
            }
        }

        protected sealed override void BeforeFlush()
        {
            Writer.Write(DbaseRecord.EndOfFile);
        }
    }
}
