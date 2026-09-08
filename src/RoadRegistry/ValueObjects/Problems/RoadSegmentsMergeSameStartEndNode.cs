namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-6: the two roads meet at both ends, so joining them would close a loop onto a single node.
public class RoadSegmentsMergeSameStartEndNode : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Merge.SameStartEndNode;

    public RoadSegmentsMergeSameStartEndNode(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2)
        : base(ProblemCode,
            new ProblemParameter("Wegsegment1Id", roadSegmentId1.ToInt32().ToString()),
            new ProblemParameter("Wegsegment2Id", roadSegmentId2.ToInt32().ToString()))
    {
    }
}
