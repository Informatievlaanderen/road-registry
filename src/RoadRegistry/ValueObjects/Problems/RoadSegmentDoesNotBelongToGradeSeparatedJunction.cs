namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-5 / VAL-7: the caller names which of the crossing roads goes under and which goes over, so both have to be roads
// this crossing is actually about.
public class RoadSegmentDoesNotBelongToGradeSeparatedJunction : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.GradeSeparatedJunction.RoadSegmentDoesNotBelong;

    public RoadSegmentDoesNotBelongToGradeSeparatedJunction(RoadSegmentId roadSegmentId, GradeSeparatedJunctionId gradeSeparatedJunctionId)
        : base(ProblemCode,
            new ProblemParameter("WegsegmentId", roadSegmentId.ToInt32().ToString()),
            new ProblemParameter("OngelijkGrondseKruisingId", gradeSeparatedJunctionId.ToInt32().ToString()))
    {
    }
}
