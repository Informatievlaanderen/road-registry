namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-4: only a road segment with status 'gerealiseerd' can be corrected back to 'gepland'. Which road segment it
// concerns comes from the problem context.
public class RoadSegmentCorrectFromRealizedToPlannedStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.CorrectFromRealizedToPlanned.StatusNotValid;

    public RoadSegmentCorrectFromRealizedToPlannedStatusNotValid()
        : base(ProblemCode.ToString())
    {
    }
}
