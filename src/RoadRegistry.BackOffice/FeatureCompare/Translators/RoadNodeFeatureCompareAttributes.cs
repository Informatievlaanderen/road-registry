namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using NetTopologySuite.Geometries;

public class RoadNodeFeatureCompareAttributes
{
    public int Id { get; init; }
    public int Type { get; init; }
    public Point Geometry { get; set; }
}
