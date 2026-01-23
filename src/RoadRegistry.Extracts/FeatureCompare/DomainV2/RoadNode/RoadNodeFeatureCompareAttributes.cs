namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;

using NetTopologySuite.Geometries;

public record RoadNodeFeatureCompareAttributes
{
    public RoadNodeId Id { get; init; }
    public Point? Geometry { get; init; }
    public RoadNodeTypeV2? Type { get; init; }
    public bool? Grensknoop { get; init; }

    public RoadNodeFeatureCompareAttributes OnlyChangedAttributes(RoadNodeFeatureCompareAttributes other)
    {
        return new RoadNodeFeatureCompareAttributes
        {
            Id = Id,
            Geometry = Geometry!.EqualsExact(other.Geometry) ? null : Geometry,
            Type = Type == other.Type ? null : Type,
            Grensknoop = Grensknoop == other.Grensknoop ? null : Grensknoop
        };
    }
}
