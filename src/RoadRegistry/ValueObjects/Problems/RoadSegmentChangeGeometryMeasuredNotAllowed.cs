namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-5: only a holder of the 'ingemeten weg' scope may change the geometry of an ingemeten road segment. This is
// reported for the road segment in the request as well as for any connected segment that would be dragged along by a
// moved road node, so it is always raised under the context of the segment that is actually ingemeten - which is not
// necessarily the one the request names.
public class RoadSegmentChangeGeometryMeasuredNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.MeasuredNotAllowed;

    public RoadSegmentChangeGeometryMeasuredNotAllowed()
        : base(ProblemCode.ToString())
    {
    }
}
