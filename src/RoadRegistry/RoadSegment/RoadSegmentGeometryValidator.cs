namespace RoadRegistry.RoadSegment;

using Extensions;
using NetTopologySuite.Geometries;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public class RoadSegmentGeometryValidator
{
    public Problems Validate(RoadSegmentId roadSegmentId, RoadSegmentGeometryDrawMethod geometryDrawMethod, MultiLineString geometry)
    {
        var problems = Problems.None;

        var line = geometry.GetSingleLineString();

        if (geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(roadSegmentId);

            return problems;
        }

        problems += line.ValidateRoadSegmentGeometry(roadSegmentId);

        return problems;
    }
}
