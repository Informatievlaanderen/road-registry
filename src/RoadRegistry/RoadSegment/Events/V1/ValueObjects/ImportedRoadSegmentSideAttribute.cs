namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

public class ImportedRoadSegmentSideAttribute
{
    public required string Municipality { get; set; }
    public required string MunicipalityNISCode { get; set; }
    public required string StreetName { get; set; }
    public required int? StreetNameId { get; set; }
}
