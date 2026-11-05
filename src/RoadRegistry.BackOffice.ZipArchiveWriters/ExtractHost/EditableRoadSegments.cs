namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

public sealed record RoadSegmentTopology(int Id, int StartNodeId, int EndNodeId);

public static class EditableRoadSegments
{
    // Which of the road segments in a contour can be handed out as editable data in a V1 extract.
    //
    // A road segment that takes part in an inwinning ('locked' or 'compleet') is not editable, and neither is a road
    // segment that shares a road node with one: that road node was already handed out in an inwinning extract, and a
    // road segment can only be editable when both its road nodes are. Every other road segment is editable.
    //
    // The inwinning road segments may lie outside the contour, as long as they share a road node with a road segment
    // inside it.
    public static IReadOnlySet<int> Determine(
        IEnumerable<RoadSegmentTopology> segmentsInContour,
        IEnumerable<RoadSegmentTopology> inwinningRoadSegments)
    {
        var inwinningRoadSegmentList = inwinningRoadSegments.ToList();
        var inwinningRoadSegmentIds = inwinningRoadSegmentList
            .Select(x => x.Id)
            .ToHashSet();
        var lockedRoadNodeIds = inwinningRoadSegmentList
            .SelectMany(x => new[] { x.StartNodeId, x.EndNodeId })
            .ToHashSet();

        return segmentsInContour
            .Where(x => !inwinningRoadSegmentIds.Contains(x.Id)
                        && !lockedRoadNodeIds.Contains(x.StartNodeId)
                        && !lockedRoadNodeIds.Contains(x.EndNodeId))
            .Select(x => x.Id)
            .ToHashSet();
    }

    // The behaviour before GAWR-7796: as soon as one road segment in the contour takes part in an inwinning, none of
    // the road segments in the contour are editable.
    public static IReadOnlySet<int> DetermineAllOrNothing(
        IEnumerable<RoadSegmentTopology> segmentsInContour,
        bool overlapsWithInwinning)
    {
        return overlapsWithInwinning
            ? new HashSet<int>()
            : segmentsInContour.Select(x => x.Id).ToHashSet();
    }

    // A grade separated junction is only editable when both its road segments are. Before GAWR-7796 one road segment
    // in the contour was enough.
    public static bool IsGradeSeparatedJunctionEditable(
        IReadOnlySet<int> editableRoadSegmentIds,
        int upperRoadSegmentId,
        int lowerRoadSegmentId,
        bool useEditableExtractDataNearInwinning)
    {
        return useEditableExtractDataNearInwinning
            ? editableRoadSegmentIds.Contains(upperRoadSegmentId) && editableRoadSegmentIds.Contains(lowerRoadSegmentId)
            : editableRoadSegmentIds.Contains(upperRoadSegmentId) || editableRoadSegmentIds.Contains(lowerRoadSegmentId);
    }
}
