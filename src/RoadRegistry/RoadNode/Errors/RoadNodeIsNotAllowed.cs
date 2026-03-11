namespace RoadRegistry.RoadNode.Errors;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;

public class RoadNodeIsNotAllowed : Error
{
    public RoadNodeIsNotAllowed(RoadSegmentIdReference segment1IdReference, RoadSegmentIdReference segment2IdReference)
        : base(ProblemCode.RoadNode.IsNotAllowed.ToString(), Enumerable.Empty<ProblemParameter>()
            .Concat(segment1IdReference.ToRoadSegmentProblemParameters("Wegsegment1"))
            .Concat(segment2IdReference.ToRoadSegmentProblemParameters("Wegsegment2"))
            .ToArray()
        )
    {
    }
}
