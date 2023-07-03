namespace RoadRegistry.BackOffice;

using System.Linq;

public static class RoadSegmentValidationExtensions
{
    public static bool IsValidForRoadSegmentEdit(this RoadSegmentMorphology morphology)
    {
        return RoadSegmentMorphology.Edit.Editable.Contains(morphology);
    }

    public static bool IsValidForRoadSegmentEdit(this RoadSegmentStatus status)
    {
        return RoadSegmentStatus.Edit.Editable.Contains(status);
    }

    public static bool IsValidForRoadSegmentEdit(this RoadSegmentSurfaceType surfaceType)
    {
        return RoadSegmentSurfaceType.Edit.Editable.Contains(surfaceType);
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
