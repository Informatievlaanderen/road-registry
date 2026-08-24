namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-6: the geometry draw method can only be changed on a road segment with status 'gepland', 'gerealiseerd' or
// 'buiten gebruik'. Which road segment it concerns comes from the problem context.
public class RoadSegmentChangeGeometryDrawMethodStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometryDrawMethod.StatusNotValid;

    public RoadSegmentChangeGeometryDrawMethodStatusNotValid()
        : base(ProblemCode.ToString())
    {
    }
}
