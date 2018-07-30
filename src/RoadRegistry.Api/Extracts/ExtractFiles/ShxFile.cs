namespace RoadRegistry.Api.Extracts.ExtractFiles
{
    using System.Collections.Generic;
    using System.Text;
    using Shaperon;

    public class ShxFile : ExtractFile
    {
        public ShxFile(string name, ShapeFileHeader header)
            : base(name, "shx", Encoding.ASCII)
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

        protected sealed override void BeforeFlush()
        { }
    }
}
