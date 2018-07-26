namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Text;
    using Projections;
    using Shaperon;

    public class DbfFile<T> : ExtractFile
        where T : DbaseRecord, new()
    {
        private readonly Encoding _encoding;
        private readonly T _dbaseRecord;

        public DbfFile(string name, Encoding encoding, DbaseFileHeader header)
            : base(name, ".dbf", encoding)
        {
            _encoding = encoding;
            _dbaseRecord = new T();
            header.Write(Writer);
        }

        public void Write(IBinaryReadableRecord record)
        {
            _dbaseRecord.FromBytes(record.DbaseRecord, _encoding);
            _dbaseRecord.Write(Writer);
        }

        protected sealed override void BeforeFlush()
        {
            Writer.Write(DbaseRecord.EndOfFile);
        }
    }
}
