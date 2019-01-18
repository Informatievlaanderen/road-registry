namespace RoadRegistry.Api.Downloads.ExtractFiles
{
    using System.IO;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class ShxFileWriter : ExtractFileWriter
    {
        public ShxFileWriter(ShapeFileHeader header, Stream writeStream)
            : base(Encoding.ASCII, writeStream)
        {
            header.Write(Writer);
        }

        public void Write(ShapeIndexRecord record)
        {
            record.Write(Writer);
        }
    }
}
