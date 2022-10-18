namespace RoadRegistry.Product.Schema.RoadSegments;

public class RoadSegmentRecord
{
    public RoadSegmentBoundingBox BoundingBox { get; set; }
    public byte[] DbaseRecord { get; set; }
    public int Id { get; set; }
    public byte[] ShapeRecordContent { get; set; }
    public int ShapeRecordContentLength { get; set; }
}
