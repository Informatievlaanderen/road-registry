namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using NetTopologySuite.Geometries;

public class RoadSegmentFeatureCompareAttributes
{
    public int Id { get; set; }
    public int StartNodeId { get; init; }
    public string MaintenanceAuthority { get; init; }
    public int EndNodeId { get; init; }
    public int? LeftStreetNameId { get; init; }
    public int Method { get; init; }
    public int Morphology { get; init; }
    public int? RightStreetNameId { get; init; }
    public int Status { get; init; }
    public int AccessRestriction { get; init; }
    public string Category { get; init; }

    public MultiLineString Geometry { get; set; }
}
