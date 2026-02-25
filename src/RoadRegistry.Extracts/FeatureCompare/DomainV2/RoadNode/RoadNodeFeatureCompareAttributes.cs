namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;

using NetTopologySuite.Geometries;

public record RoadNodeFeatureCompareAttributes
{
    public required RoadNodeId RoadNodeId { get; init; }
    public required Point Geometry { get; init; }
    public bool? Grensknoop { get; init; }

    public RoadNodeFeatureCompareAttributes OnlyChangedAttributes(RoadNodeFeatureCompareAttributes other, Point extractGeometry)
    {
        return new RoadNodeFeatureCompareAttributes
        {
            RoadNodeId = RoadNodeId,
            Geometry = Geometry.EqualsExact(other.Geometry) ? extractGeometry : Geometry,
            Grensknoop = Grensknoop == other.Grensknoop ? null : Grensknoop
        };
    }
}
