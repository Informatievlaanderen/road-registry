namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// A decentral manager may only edit 'ingeschetste' road segments. Which road segment is measured comes from the
// problem context - not necessarily the one the request names.
public class RoadSegmentMeasuredNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.MeasuredNotAllowed;

    public RoadSegmentMeasuredNotAllowed()
        : base(ProblemCode.ToString())
    {
    }
}
