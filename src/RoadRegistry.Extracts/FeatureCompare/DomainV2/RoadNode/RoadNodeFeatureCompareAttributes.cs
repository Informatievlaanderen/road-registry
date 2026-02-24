namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;

using NetTopologySuite.Geometries;

public record RoadNodeFeatureCompareAttributes
{
    public RoadNodeId Id { get; init; }
    public Point Geometry { get; init; }
    public bool GeometryChanged { get; init; }
    public bool? Grensknoop { get; init; }

    public RoadNodeFeatureCompareAttributes OnlyChangedAttributes(RoadNodeFeatureCompareAttributes other)
    {
        return other with
        {
            GeometryChanged = !Geometry.EqualsExact(other.Geometry),
            Grensknoop = Grensknoop == other.Grensknoop ? null : Grensknoop
        };
    }
}
