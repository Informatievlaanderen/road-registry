namespace RoadRegistry.Integration.Schema.RoadNodes.Version;

using System;
using NetTopologySuite.Geometries;

public class RoadNodeVersion
{
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string TypeLabel { get; set; }
    public int Version { get; set; }
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public bool IsRemoved { get; set; }

    public Geometry Geometry { get; set; }
    public double? BoundingBoxMaximumX { get; set; }
    public double? BoundingBoxMaximumY { get; set; }
    public double? BoundingBoxMinimumX { get; set; }
    public double? BoundingBoxMinimumY { get; set; }

    public DateTimeOffset VersionTimestamp { get; set; }
    public DateTimeOffset CreatedOnTimestamp { get; set; }

    public RoadNodeVersion WithBoundingBox(RoadNodeBoundingBox value)
    {
        BoundingBoxMaximumX = value.MaximumX;
        BoundingBoxMaximumY = value.MaximumY;
        BoundingBoxMinimumX = value.MinimumX;
        BoundingBoxMinimumY = value.MinimumY;
        return this;
    }
}
