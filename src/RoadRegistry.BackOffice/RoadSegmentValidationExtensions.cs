namespace RoadRegistry.BackOffice;

using System.Linq;

public static class RoadSegmentValidationExtensions
{
    public static bool IsValidForRoadSegmentOutline(this RoadSegmentMorphology morphology)
    {
        return RoadSegmentMorphology.Outlined.All.Contains(morphology);
    }

    public static bool IsValidForRoadSegmentOutline(this RoadSegmentStatus status)
    {
        return RoadSegmentStatus.Outlined.All.Contains(status);
    }

    public static bool IsValidForRoadSegmentOutline(this RoadSegmentSurfaceType surfaceType)
    {
        return RoadSegmentSurfaceType.Outlined.All.Contains(surfaceType);
    }

    public static bool IsValidStartRoadNodeIdForRoadSegmentOutline(this int nodeId)
    {
        return nodeId == RoadNodeId.Zero;
    }

    public static bool IsValidEndRoadNodeIdForRoadSegmentOutline(this int nodeId)
    {
        return nodeId == RoadNodeId.Zero;
    }

    public static bool IsAllowed(this RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        return RoadSegmentGeometryDrawMethod.Allowed.Contains(geometryDrawMethod);
    }
}
