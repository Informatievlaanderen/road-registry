namespace RoadRegistry.Extracts.Infrastructure.ShapeFile;

using System.Text;

public sealed class Lambert08ShapeFileRecordWriter : ShapeFileRecordWriter
{
    public Lambert08ShapeFileRecordWriter(Encoding encoding)
        : base(encoding, WellknownSrids.Lambert08)
    {
    }
}
