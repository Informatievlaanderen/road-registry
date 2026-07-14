namespace RoadRegistry;

using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects;

public static class JunctionGeometryCalculator
{
    public static JunctionGeometry? Calculate(RoadSegmentGeometry geometry1, RoadSegmentGeometry geometry2)
    {
        Geometry intersection;
        try
        {
            intersection = geometry1.Value.Intersection(geometry2.Value);
        }
        catch (TopologyException)
        {
            return null;
        }

        if (intersection.IsEmpty)
        {
            return null;
        }

        return JunctionGeometry.Create(geometry1.Value.Factory.CreatePoint(intersection.Coordinate.RoundToCm()));
    }
}
