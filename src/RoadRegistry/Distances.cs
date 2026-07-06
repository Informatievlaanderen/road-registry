namespace RoadRegistry;

public static class Distances
{
    public static readonly double TooClose = 2.0; // 2 meters
    public static readonly double RoadSegmentV2MinimumLength = 1.0;
    public static readonly double TooLongSegmentLength = 100000.0; // 100km
    public static readonly double MinimumDistanceBetweenVertices = 0.15;
    public static readonly double RoadSegmentSplitMaximumDistanceToRoadSegment = 1.0; // 1 meter
    public static readonly double RoadSegmentSplitMinimumDistanceToRoadNode = 1.0; // 1 meter
}
