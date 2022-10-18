namespace RoadRegistry.Editor.Schema.RoadSegments;

using NetTopologySuite.Geometries;

public class RoadSegmentRecord
{
    public RoadSegmentBoundingBox BoundingBox { get; set; }
    public byte[] DbaseRecord { get; set; }
    public int EndNodeId { get; set; }
    public Geometry Geometry { get; set; }
    public int Id { get; set; }
    public byte[] ShapeRecordContent { get; set; }
    public int ShapeRecordContentLength { get; set; }
    public int StartNodeId { get; set; }
}
