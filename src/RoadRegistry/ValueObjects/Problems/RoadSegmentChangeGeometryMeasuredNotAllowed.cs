namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-5: a decentral manager may only change the geometry of 'ingeschetste' road segments. This is reported for the
// road segment in the request as well as for any connected segment that would be dragged along by a moved road node,
// which is why the identifier is always carried.
public class RoadSegmentChangeGeometryMeasuredNotAllowed : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.MeasuredNotAllowed;

    public RoadSegmentChangeGeometryMeasuredNotAllowed(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
