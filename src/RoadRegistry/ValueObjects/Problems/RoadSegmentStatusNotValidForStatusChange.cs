namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// A status change only applies to a road segment that is in the status it changes away from. Which road segment it
// concerns comes from the problem context; the status it should have been in travels as a parameter, so the two
// status changes that predate the table in RoadSegmentStatusChange can keep the problem codes they were published
// with while everything else shares one.
public class RoadSegmentStatusNotValidForStatusChange : Error
{
    public RoadSegmentStatusNotValidForStatusChange(ProblemCode problemCode, RoadSegmentStatusV2 requiredStatus)
        : base(problemCode.ToString(), new ProblemParameter("RequiredStatus", requiredStatus.ToDutchString()))
    {
    }
}
