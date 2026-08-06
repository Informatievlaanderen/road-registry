namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-35: attribute values can only be changed on a road segment with status 'gepland', 'gerealiseerd' or
// 'buiten gebruik'. Which road segment it concerns comes from the problem context.
public class RoadSegmentChangeAttributesStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeAttributes.StatusNotValid;

    public RoadSegmentChangeAttributesStatusNotValid()
        : base(ProblemCode.ToString())
    {
    }
}
