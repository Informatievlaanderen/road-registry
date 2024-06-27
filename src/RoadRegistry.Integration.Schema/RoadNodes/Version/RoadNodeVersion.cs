namespace RoadRegistry.Integration.Schema.RoadNodes.Version;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Utilities;
using NetTopologySuite.Geometries;
using NodaTime;

public class RoadNodeVersion
{
    public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
    public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

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

    public string VersionAsString { get; set; }
    private DateTimeOffset VersionTimestampAsDateTimeOffset { get; set; }

    public Instant VersionTimestamp
    {
        get => Instant.FromDateTimeOffset(VersionTimestampAsDateTimeOffset);
        set
        {
            VersionTimestampAsDateTimeOffset = value.ToDateTimeOffset();
            VersionAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
        }
    }

    public string CreatedOnAsString { get; set; }
    private DateTimeOffset CreatedOnTimestampAsDateTimeOffset { get; set; }

    public Instant CreatedOnTimestamp
    {
        get => Instant.FromDateTimeOffset(CreatedOnTimestampAsDateTimeOffset);
        set
        {
            CreatedOnTimestampAsDateTimeOffset = value.ToDateTimeOffset();
            CreatedOnAsString = new Rfc3339SerializableDateTimeOffset(value.ToBelgianDateTimeOffset()).ToString();
        }
    }

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
