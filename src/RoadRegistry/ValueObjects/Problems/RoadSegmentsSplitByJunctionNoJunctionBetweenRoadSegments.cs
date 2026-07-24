namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-4: there must be a grade (gelijkgrondse) or grade-separated (ongelijkgrondse) junction between the two road segments.
public class RoadSegmentsSplitByJunctionNoJunctionBetweenRoadSegments : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.SplitByJunction.NoJunctionBetweenRoadSegments;

    public RoadSegmentsSplitByJunctionNoJunctionBetweenRoadSegments(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2)
        : base(ProblemCode.ToString(),
            new ProblemParameter("Wegsegment1Id", roadSegmentId1.ToInt32().ToString()),
            new ProblemParameter("Wegsegment2Id", roadSegmentId2.ToInt32().ToString()))
    {
    }
}
