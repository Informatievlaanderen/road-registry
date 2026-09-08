namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-5: one road cannot be half measured and half sketched, so the two that are to become one have to agree on how
// they were drawn. Changing the draw method of one of them first is what makes the merge possible.
public class RoadSegmentsMergeGeometryDrawMethodNotEqual : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Merge.GeometryDrawMethodNotEqual;

    public RoadSegmentsMergeGeometryDrawMethodNotEqual(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2)
        : base(ProblemCode,
            new ProblemParameter("Wegsegment1Id", roadSegmentId1.ToInt32().ToString()),
            new ProblemParameter("Wegsegment2Id", roadSegmentId2.ToInt32().ToString()))
    {
    }
}
