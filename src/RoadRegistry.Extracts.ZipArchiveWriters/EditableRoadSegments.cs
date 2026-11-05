namespace RoadRegistry.Extracts.ZipArchiveWriters;

using RoadRegistry.Extracts.Projections;
using RoadRegistry.ValueObjects;

public static class EditableRoadSegments
{
    // Which road segments of a contour a bijhouding extract hands out as bewerkbare data: those with inwinningsstatus
    // 'compleet' that share no road node with a road segment that is still 'nietGestart' or 'locked'. That other road
    // segment may lie outside the contour, so the connected road segments are passed in as well.
    //
    // A road segment handed out as bewerkbare data has to have a begin- and eindknoop the bijwerker may change, and a
    // road node is only that when every road segment it connects is compleet.
    public static IReadOnlySet<RoadSegmentId> Select(
        IReadOnlyCollection<RoadSegmentExtractItem> roadSegmentsInContour,
        IReadOnlyCollection<RoadSegmentExtractItem> connectedRoadSegments,
        IReadOnlyDictionary<RoadSegmentId, Inwinningsstatus> inwinningsstatus)
    {
        ArgumentNullException.ThrowIfNull(roadSegmentsInContour);
        ArgumentNullException.ThrowIfNull(connectedRoadSegments);
        ArgumentNullException.ThrowIfNull(inwinningsstatus);

        bool IsCompleet(RoadSegmentExtractItem roadSegment) =>
            inwinningsstatus.TryGetValue(roadSegment.RoadSegmentId, out var status) && status == Inwinningsstatus.Compleet;

        var lockedRoadNodeIds = roadSegmentsInContour
            .Concat(connectedRoadSegments)
            .Where(roadSegment => !IsCompleet(roadSegment))
            .SelectMany(RoadNodeIdsOf)
            .ToHashSet();

        return roadSegmentsInContour
            .Where(IsCompleet)
            .Where(roadSegment => !RoadNodeIdsOf(roadSegment).Any(lockedRoadNodeIds.Contains))
            .Select(roadSegment => roadSegment.RoadSegmentId)
            .ToHashSet();
    }

    public static IEnumerable<RoadNodeId> RoadNodeIdsOf(RoadSegmentExtractItem roadSegment)
    {
        if (roadSegment.StartNodeId is { } startNodeId)
        {
            yield return startNodeId;
        }

        if (roadSegment.EndNodeId is { } endNodeId)
        {
            yield return endNodeId;
        }
    }
}
