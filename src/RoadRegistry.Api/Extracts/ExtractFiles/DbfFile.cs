namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Text;
    using ExtractFiles;
    using Shaperon;

    public class DbfFile : ExtractFile
    {
        public DbfFile(string name, Encoding encoding)
            : base(name, ".dbf", encoding)
        { }

        public void Write(byte value)
        {
            Writer.Write(value);
        }

        public void Write(DbaseFileHeader header)
        {
            header.Write(Writer);
        }

        public void Write(DbaseRecord record)
        {
            record.Write(Writer);
        }
    }
}
