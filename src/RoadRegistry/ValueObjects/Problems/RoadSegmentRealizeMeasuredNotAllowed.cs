namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-9: a decentral manager may only realize 'ingeschetste' road segments. A 'gepland' segment is rarely
// 'ingemeten', but the line is drawn the same way for every status transition: only a central manager gets to do it.
public class RoadSegmentRealizeMeasuredNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Realize.MeasuredNotAllowed;

    public RoadSegmentRealizeMeasuredNotAllowed()
        : base(ProblemCode.ToString())
    {
    }
}
