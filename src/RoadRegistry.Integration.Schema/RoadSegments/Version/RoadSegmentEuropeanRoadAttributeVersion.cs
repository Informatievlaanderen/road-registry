namespace RoadRegistry.Integration.Schema.RoadSegments.Version;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.Utilities;
using NodaTime;

public class RoadSegmentEuropeanRoadAttributeVersion
{
    public const string VersionTimestampBackingPropertyName = nameof(VersionTimestampAsDateTimeOffset);
    public const string CreatedOnTimestampBackingPropertyName = nameof(CreatedOnTimestampAsDateTimeOffset);

    public required long Position { get; init; }
    public required int Id { get; init; }
    public required int RoadSegmentId { get; set; }

    public string Number { get; set; }
    public bool IsRemoved { get; set; }
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }

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

    public RoadSegmentEuropeanRoadAttributeVersion Clone(long newPosition)
    {
        var newItem = new RoadSegmentEuropeanRoadAttributeVersion
        {
            Position = newPosition,
            Id = Id,
            RoadSegmentId = RoadSegmentId,

            Number = Number,
            IsRemoved = IsRemoved,
            OrganizationId = OrganizationId,
            OrganizationName = OrganizationName,

            VersionTimestamp = VersionTimestamp,
            CreatedOnTimestamp = CreatedOnTimestamp
        };

        return newItem;
    }
}
