namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

using NetTopologySuite.Geometries;

public record RoadNodeFeatureCompareAttributes
{
    public RoadNodeId Id { get; init; }
    public Point? Geometry { get; init; }
    public RoadNodeType? Type { get; init; }

    public RoadNodeFeatureCompareAttributes OnlyChangedAttributes(RoadNodeFeatureCompareAttributes other)
    {
        return new RoadNodeFeatureCompareAttributes
        {
            Id = Id,
            Geometry = Geometry!.EqualsExact(other.Geometry) ? null : Geometry,
            Type = Type == other.Type ? null : Type
        };
    }
}
