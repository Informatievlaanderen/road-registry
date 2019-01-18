namespace RoadRegistry.Api.Downloads.ExtractFiles
{
    using System.IO;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ShpFileWriter : ExtractFileWriter
    {
        public ShpFileWriter(ShapeFileHeader header, Stream writeStream)
            : base(Encoding.ASCII, writeStream)
        {
            header.Write(Writer);
        }

        public void Write(ShapeRecord record)
        {
            record.Write(Writer);
        }
    }
}
