namespace RoadRegistry;

public static class Distances
{
    public static readonly double TooClose = 2.0; // 2 meters
    public static readonly double RoadSegmentV2MinimumLength = 1.0;
    public static readonly double TooLongSegmentLength = 100000.0; // 100km
    public static readonly double MinimumDistanceBetweenVertices = 0.15;
    public static readonly double RoadSegmentSplitMaximumDistanceToRoadSegment = 1.0; // 1 meter
    public static readonly double RoadSegmentSplitMinimumDistanceToRoadNode = 1.0; // 1 meter

    // Changing a road segment geometry drags the road nodes on its endpoints along with it. A moved endpoint has to
    // stay at least this far away from any other road node, otherwise two nodes end up on top of each other.
    public static readonly double RoadSegmentChangeGeometryMinimumDistanceToRoadNode = 1.0; // 1 meter

    // Realizing a 'gepland' segment knots it into the network: an endpoint with an existing road node this close is
    // snapped onto it, an endpoint without one gets an 'eindknoop' of its own.
    public static readonly double RoadSegmentRealizeMaximumDistanceToRoadNode = 1.0; // 1 meter

    // A stretch a dynamically segmented attribute value covers may not be shorter than this. When a geometry change
    // squeezes one below it, the stretch lapses and its neighbour takes over rather than the segment being refused.
    public static readonly double RoadSegmentDynamicAttributeMinimumLength = 1.0; // 1 meter

    // ...and it may not drag a node arbitrarily far. An 'eindknoop' terminates a road and can legitimately be
    // extended over a longer distance; a node that other segments hang off may not wander.
    public static readonly double RoadNodeMaximumMoveDistance = 20.0; // 20 meters
    public static readonly double EndRoadNodeMaximumMoveDistance = 100.0; // 100 meters
}
