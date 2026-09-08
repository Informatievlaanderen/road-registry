namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-8: joining the two roads would run the result across a third road more than once, which the register cannot
// record as a single crossing.
public class RoadSegmentsMergeMultipleIntersections : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Merge.MultipleIntersections;

    public RoadSegmentsMergeMultipleIntersections(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2, RoadSegmentId otherRoadSegmentId)
        : base(ProblemCode,
            new ProblemParameter("Wegsegment1Id", roadSegmentId1.ToInt32().ToString()),
            new ProblemParameter("Wegsegment2Id", roadSegmentId2.ToInt32().ToString()),
            new ProblemParameter("OtherWegsegmentId", otherRoadSegmentId.ToInt32().ToString()))
    {
    }
}
