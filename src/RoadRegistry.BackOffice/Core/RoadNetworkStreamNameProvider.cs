namespace RoadRegistry.BackOffice.Core;

using Framework;
using RoadRegistry.RoadSegment.ValueObjects;

public static class RoadNetworkStreamNameProvider
{
    public static readonly StreamName Default = new("roadnetwork") { SupportsSnapshot = true };

    public static StreamName ForOutlinedRoadSegment(RoadSegmentId roadSegmentId)
    {
        return new StreamName($"roadsegment-outline-{roadSegmentId}");
    }

    public static StreamName Get(RoadSegmentId? roadSegmentId, RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        if (roadSegmentId is not null && geometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            return ForOutlinedRoadSegment(roadSegmentId.Value);
        }

        return Default;
    }
}
