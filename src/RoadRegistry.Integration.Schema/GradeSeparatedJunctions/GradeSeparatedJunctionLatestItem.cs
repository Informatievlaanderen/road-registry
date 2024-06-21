using System;

namespace RoadRegistry.Integration.Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionLatestItem
{
    public int Id { get; set; }
    public int LowerRoadSegmentId { get; set; }
    public int UpperRoadSegmentId { get; set; }
    public int TypeId { get; set; }
    public string TypeLabel { get; set; }
    public string BeginOrganizationId { get; set; }
    public string BeginOrganizationName { get; set; }
    public bool IsRemoved { get; set; }

    public DateTimeOffset VersionTimestamp { get; set; }
    public DateTimeOffset CreatedOnTimestamp { get; set; }
}
