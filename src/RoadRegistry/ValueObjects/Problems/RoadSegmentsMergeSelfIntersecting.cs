namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-7: joining the two roads would run the result across itself.
public class RoadSegmentsMergeSelfIntersecting : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Merge.SelfIntersecting;

    public RoadSegmentsMergeSelfIntersecting(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2)
        : base(ProblemCode,
            new ProblemParameter("Wegsegment1Id", roadSegmentId1.ToInt32().ToString()),
            new ProblemParameter("Wegsegment2Id", roadSegmentId2.ToInt32().ToString()))
    {
    }
}
