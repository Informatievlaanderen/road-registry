namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Collections.Generic;
    using System.Text;
    using ExtractFiles;
    using Shaperon;

    public class ShxFile : ExtractFile
    {
        public ShxFile(string name)
            : base(name, ".shx", Encoding.ASCII)
        { }

        public void Write(ShapeFileHeader header)
        {
            header.Write(Writer);
        }

        public void Write(IEnumerable<ShapeIndexRecord> records)
        {
            foreach (var record in records)
            {
                record.Write(Writer);
            }
        }
    }
}
