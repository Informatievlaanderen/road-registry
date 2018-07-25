namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Collections.Generic;
    using System.Text;
    using ExtractFiles;
    using Shaperon;

    public class ShpFile : ExtractFile
    {
        public ShpFile(string name)
            : base(name, ".shp", Encoding.ASCII)
        { }

        public void Write(ShapeFileHeader header)
        {
            header.Write(Writer);
        }

        public void Write(IEnumerable<ShapeRecord> records)
        {
            foreach (var record in records)
            {
                record.Write(Writer);
            }
        }
    }
}
