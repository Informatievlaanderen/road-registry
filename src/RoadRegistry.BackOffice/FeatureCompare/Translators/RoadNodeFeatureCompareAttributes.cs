namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeFeatureCompareAttributes
{
    public int TYPE { get; init; }
    public int WK_OIDN { get; init; }
    public Point Geometry { get; set; }
}
