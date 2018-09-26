namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Collections.Generic;
    using System.Text;
    using Aiv.Vbr.Shaperon;

    public class ShpFile : ExtractFile
    {
        public ShpFile(string name, ShapeFileHeader header)
            : base(name, "shp", Encoding.ASCII)
        {
            header.Write(Writer);
        }

        public void Write(IEnumerable<ShapeRecord> records)
        {
            foreach (var record in records)
            {
                Write(record);
            }
        }

        public void Write(ShapeRecord record)
        {
            record.Write(Writer);
        }

        protected sealed override void BeforeFlush()
        { }
    }
}
