namespace RoadRegistry.BackOffice.Core;

using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

public static class ScopedRoadNetworkViewExtensions
{
    public static IEnumerable<KeyValuePair<RoadSegmentId, RoadSegment>> FindIntersectingRoadSegments(
        this IScopedRoadNetworkView scopedRoadNetworkView,
        RoadSegmentId intersectsWithId,
        MultiLineString intersectsWithGeometry,
        params RoadNodeId[] roadNodeIdsNotInCommon)
    {
        //TODO-rik WR-929 inschetste wegsegmenten hier niet in rekening houden
        return scopedRoadNetworkView
            .Segments
            .Where(segment => segment.Value.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            .Where(segment => segment.Key != intersectsWithId)
            .Where(segment => segment.Value.Geometry.Intersects(intersectsWithGeometry))
            .Where(segment => !segment.Value.Nodes.Any(roadNodeIdsNotInCommon.Contains));
    }

    public static IEnumerable<KeyValuePair<RoadSegmentId, RoadSegment>> FindIntersectingRoadSegments(
        this IScopedRoadNetworkView scopedRoadNetworkView,
        AddRoadSegment addRoadSegment)
    {
        return scopedRoadNetworkView
            .FindIntersectingRoadSegments(
                addRoadSegment.Id,
                addRoadSegment.Geometry,
                addRoadSegment.StartNodeId,
                addRoadSegment.EndNodeId);
    }

    public static IEnumerable<KeyValuePair<RoadSegmentId, RoadSegment>> FindIntersectingRoadSegments(
        this IScopedRoadNetworkView scopedRoadNetworkView,
        ModifyRoadSegment modifyRoadSegment)
    {
        return scopedRoadNetworkView
            .FindIntersectingRoadSegments(
                modifyRoadSegment.Id,
                modifyRoadSegment.Geometry,
                modifyRoadSegment.StartNodeId,
                modifyRoadSegment.EndNodeId);
    }
}
