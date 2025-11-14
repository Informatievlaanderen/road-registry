namespace RoadRegistry.RoadSegment;

using BackOffice;
using BackOffice.Core;
using NetTopologySuite.Geometries;
using ValueObjects;

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
