namespace RoadRegistry.Infrastructure;

using System.Linq;

public static class RoadSegmentValidationExtensions
{
    public static bool IsValid(this RoadSegmentMorphology morphology, bool outlined)
    {
        return !outlined || IsValidForEdit(morphology);
    }
    public static bool IsValid(this RoadSegmentStatus status, bool outlined)
    {
        return !outlined || IsValidForEdit(status);
    }

    public static bool IsValidForEdit(this RoadSegmentMorphology morphology)
    {
        return RoadSegmentMorphology.Edit.Editable.Contains(morphology);
    }

    public static bool IsValidForEdit(this RoadSegmentStatus status)
    {
        return RoadSegmentStatus.Edit.Editable.Contains(status);
    }

    public static bool IsValidForEdit(this RoadSegmentCategory status)
    {
        return RoadSegmentCategory.Edit.Editable.Contains(status);
    }

    public static bool IsValidForEdit(this RoadSegmentLaneCount laneCount)
    {
        return laneCount != 0;
    }

    public static bool IsValidForEdit(this RoadSegmentWidth width)
    {
        return width != 0;
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
