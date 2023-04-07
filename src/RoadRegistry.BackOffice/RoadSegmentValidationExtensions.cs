namespace RoadRegistry.BackOffice;

using System.Linq;

public static class RoadSegmentValidationExtensions
{
    public static bool IsValidForRoadSegmentOutline(this RoadSegmentMorphology morphology)
    {
        return RoadSegmentMorphology.AllOutlined.Contains(morphology);
    }

    public static bool IsValidForRoadSegmentOutline(this RoadSegmentStatus status)
    {
        return RoadSegmentStatus.AllOutlined.Contains(status);
    }

    public static bool IsValidStartRoadNodeIdForRoadSegmentOutline(this int nodeId)
    {
        return nodeId == 0;
    }

    public static bool IsValidEndRoadNodeIdForRoadSegmentOutline(this int nodeId)
    {
        return nodeId == 0;
    }

    public static bool IsAllowed(this RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        return RoadSegmentGeometryDrawMethod.Allowed.Contains(geometryDrawMethod);
    }
}
