namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Text;
    using Shaperon;

    public class DbfFile : ExtractFile
    {
        public DbfFile(string name, Encoding encoding, DbaseFileHeader header)
            : base(name, ".dbf", encoding)
        {
            header.Write(Writer);
        }

        public void Write(byte value)
        {
            Writer.Write(value);
        }

        public void Write(DbaseRecord record)
        {
            record.Write(Writer);
        }
    }
}
