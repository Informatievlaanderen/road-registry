namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-35: attribute values can only be changed on a road segment with status 'gepland', 'gerealiseerd' or
// 'buiten gebruik'.
public class RoadSegmentChangeAttributesStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeAttributes.StatusNotValid;

    public RoadSegmentChangeAttributesStatusNotValid(RoadSegmentId identifier)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()))
    {
    }
}
