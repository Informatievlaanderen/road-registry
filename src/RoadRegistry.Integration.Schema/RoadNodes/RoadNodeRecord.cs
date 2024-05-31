namespace RoadRegistry.Integration.Schema.RoadNodes;

using NetTopologySuite.Geometries;

public class RoadNodeRecord
{
    public double? BoundingBoxMaximumX { get; set; }
    public double? BoundingBoxMaximumY { get; set; }
    public double? BoundingBoxMinimumX { get; set; }
    public double? BoundingBoxMinimumY { get; set; }
    public byte[] DbaseRecord { get; set; }
    public Geometry Geometry { get; set; }
    public int Id { get; set; }
    public byte[] ShapeRecordContent { get; set; }
    public int ShapeRecordContentLength { get; set; }
    
    public RoadNodeBoundingBox GetBoundingBox() => BoundingBoxMaximumX is not null
        ? new()
        {
            MaximumX = BoundingBoxMaximumX!.Value,
            MaximumY = BoundingBoxMaximumY!.Value,
            MinimumX = BoundingBoxMinimumX!.Value,
            MinimumY = BoundingBoxMinimumY!.Value
        }
        : null;
    public RoadNodeRecord WithBoundingBox(RoadNodeBoundingBox value)
    {
        BoundingBoxMaximumX = value.MaximumX;
        BoundingBoxMaximumY = value.MaximumY;
        BoundingBoxMinimumX = value.MinimumX;
        BoundingBoxMinimumY = value.MinimumY;
        return this;
    }
}
