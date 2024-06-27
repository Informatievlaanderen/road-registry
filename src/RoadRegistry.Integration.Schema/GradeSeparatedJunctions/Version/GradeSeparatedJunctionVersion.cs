using System;

namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions.Version;

public class GradeSeparatedJunctionVersion
{
    public required long Position { get; init; }
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public int UpperRoadSegmentId { get; set; }
    public int TypeId { get; set; }
    public string TypeLabel { get; set; }
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public bool IsRemoved { get; set; }

    public DateTimeOffset VersionTimestamp { get; set; }
    public DateTimeOffset CreatedOnTimestamp { get; set; }

    public GradeSeparatedJunctionVersion CloneAndApplyEventInfo(
        long newPosition,
        Action<GradeSeparatedJunctionVersion> applyEventInfo)
    {
        var newVersion = new GradeSeparatedJunctionVersion
        {
            Position = newPosition,
            Id = Id,
            LowerRoadSegmentId = LowerRoadSegmentId,
            UpperRoadSegmentId = UpperRoadSegmentId,
            TypeId = TypeId,
            TypeLabel = TypeLabel,
            OrganizationId = OrganizationId,
            OrganizationName = OrganizationName,
            IsRemoved = IsRemoved,
            VersionTimestamp = VersionTimestamp,
            CreatedOnTimestamp = CreatedOnTimestamp
        };

        applyEventInfo(newVersion);

        return newVersion;
    }
}
