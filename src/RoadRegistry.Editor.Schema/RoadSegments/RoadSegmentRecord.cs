namespace RoadRegistry.Editor.Schema.RoadSegments;

using NetTopologySuite.Geometries;

public class RoadSegmentRecord
{
    public double? BoundingBoxMaximumM { get; set; }
    public double? BoundingBoxMaximumX { get; set; }
    public double? BoundingBoxMaximumY { get; set; }
    public double? BoundingBoxMinimumM { get; set; }
    public double? BoundingBoxMinimumX { get; set; }
    public double? BoundingBoxMinimumY { get; set; }
    public byte[] DbaseRecord { get; set; }
    public int EndNodeId { get; set; }
    public Geometry Geometry { get; set; }
    public int Id { get; set; }
    public byte[] ShapeRecordContent { get; set; }
    public int ShapeRecordContentLength { get; set; }
    public int StartNodeId { get; set; }
    public string LastEventHash { get; set; }
    public bool IsRemoved { get; set; }

    public RoadSegmentBoundingBox GetBoundingBox() => BoundingBoxMaximumX is not null
        ? new()
        {
            MaximumX = BoundingBoxMaximumX!.Value,
            MaximumY = BoundingBoxMaximumY!.Value,
            MaximumM = BoundingBoxMaximumM!.Value,
            MinimumX = BoundingBoxMinimumX!.Value,
            MinimumY = BoundingBoxMinimumY!.Value,
            MinimumM = BoundingBoxMinimumM!.Value
        }
        : null;
    public RoadSegmentRecord WithBoundingBox(RoadSegmentBoundingBox value)
    {
        BoundingBoxMaximumX = value.MaximumX;
        BoundingBoxMaximumY = value.MaximumY;
        BoundingBoxMaximumM = value.MaximumM;
        BoundingBoxMinimumX = value.MinimumX;
        BoundingBoxMinimumY = value.MinimumY;
        BoundingBoxMinimumM = value.MinimumM;
        return this;
    }
}
