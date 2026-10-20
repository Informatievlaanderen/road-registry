namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-5: only a holder of the 'geschetste weg' scope may change the geometry of an ingeschetst road segment. This is
// reported for the road segment in the request as well as for any connected segment that would be dragged along by a
// moved road node, so it is always raised under the context of the segment that is actually ingeschetst - which is not
// necessarily the one the request names.
public class RoadSegmentChangeGeometryOutlinedNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.OutlinedNotAllowed;

    public RoadSegmentChangeGeometryOutlinedNotAllowed()
        : base(ProblemCode.ToString())
    {
    }
}
