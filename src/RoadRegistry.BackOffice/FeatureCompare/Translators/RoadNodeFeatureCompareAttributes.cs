namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using NetTopologySuite.Geometries;

public record RoadNodeFeatureCompareAttributes
{
    public RoadNodeId Id { get; init; }
    public RoadNodeType Type { get; init; }
    public Point Geometry { get; init; }
}
