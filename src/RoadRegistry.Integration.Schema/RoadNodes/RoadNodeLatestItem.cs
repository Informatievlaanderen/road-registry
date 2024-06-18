namespace RoadRegistry.Integration.Schema.RoadNodes;

using System;
using NetTopologySuite.Geometries;

public class RoadNodeLatestItem
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string TypeLabel { get; set; }
    public int Version { get; set; }
    public string BeginOrganizationId { get; set; }
    public string BeginOrganizationName { get; set; }
    public bool IsRemoved { get; set; }

    public Geometry Geometry { get; set; }
    public double? BoundingBoxMaximumX { get; set; }
    public double? BoundingBoxMaximumY { get; set; }
    public double? BoundingBoxMinimumX { get; set; }
    public double? BoundingBoxMinimumY { get; set; }

    public DateTimeOffset VersionTimestamp { get; set; }
    public DateTimeOffset CreatedOnTimestamp { get; set; }

    public RoadNodeBoundingBox GetBoundingBox() => BoundingBoxMaximumX is not null
        ? new()
        {
            MaximumX = BoundingBoxMaximumX!.Value,
            MaximumY = BoundingBoxMaximumY!.Value,
            MinimumX = BoundingBoxMinimumX!.Value,
            MinimumY = BoundingBoxMinimumY!.Value
        }
        : null;

    public RoadNodeLatestItem WithBoundingBox(RoadNodeBoundingBox value)
    {
        BoundingBoxMaximumX = value.MaximumX;
        BoundingBoxMaximumY = value.MaximumY;
        BoundingBoxMinimumX = value.MinimumX;
        BoundingBoxMinimumY = value.MinimumY;
        return this;
    }
}
