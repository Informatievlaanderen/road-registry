namespace RoadRegistry.BackOffice.FeatureCompare.V1.Translators;

using NetTopologySuite.Geometries;

public record RoadNodeFeatureCompareAttributes
{
    public RoadNodeId Id { get; init; }
    public RoadNodeType Type { get; init; }
    public Point Geometry { get; init; }
}
