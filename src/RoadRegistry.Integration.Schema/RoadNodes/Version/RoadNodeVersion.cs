namespace RoadRegistry.Integration.Schema.RoadNodes.Version;

using System;
using NetTopologySuite.Geometries;

public class RoadNodeVersion
{
    public required long Position { get; init; }
    public int Id { get; set; }
    public int TypeId { get; set; }
    public string TypeLabel { get; set; }
    public int Version { get; set; }
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public Geometry Geometry { get; set; }
    public double? BoundingBoxMaximumX { get; set; }
    public double? BoundingBoxMaximumY { get; set; }
    public double? BoundingBoxMinimumX { get; set; }
    public double? BoundingBoxMinimumY { get; set; }
    public bool IsRemoved { get; set; }
    public DateTimeOffset VersionTimestamp { get; set; }
    public DateTimeOffset CreatedOnTimestamp { get; set; }

    public RoadNodeVersion CloneAndApplyEventInfo(
        long newPosition,
        Action<RoadNodeVersion> applyEventInfo)
    {
        var newVersion = new RoadNodeVersion
        {
            Position = newPosition,
            Id = Id,
            TypeId = TypeId,
            TypeLabel = TypeLabel,
            Version = Version,
            OrganizationId = OrganizationId,
            OrganizationName = OrganizationName,
            Geometry = Geometry,
            BoundingBoxMaximumX = BoundingBoxMaximumX,
            BoundingBoxMaximumY = BoundingBoxMaximumY,
            BoundingBoxMinimumX = BoundingBoxMinimumX,
            BoundingBoxMinimumY = BoundingBoxMinimumY,
            IsRemoved = IsRemoved,
            VersionTimestamp = VersionTimestamp,
            CreatedOnTimestamp = CreatedOnTimestamp
        };

        applyEventInfo(newVersion);

        return newVersion;
    }

    public RoadNodeVersion WithBoundingBox(RoadNodeBoundingBox value)
    {
        BoundingBoxMaximumX = value.MaximumX;
        BoundingBoxMaximumY = value.MaximumY;
        BoundingBoxMinimumX = value.MinimumX;
        BoundingBoxMinimumY = value.MinimumY;
        return this;
    }
}
