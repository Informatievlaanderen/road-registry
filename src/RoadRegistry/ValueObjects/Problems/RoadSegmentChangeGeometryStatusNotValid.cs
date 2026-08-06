namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

// VAL-4: the geometry can only be changed on a road segment with status 'gepland', 'gerealiseerd' or
// 'buiten gebruik'. Which road segment it concerns comes from the problem context.
public class RoadSegmentChangeGeometryStatusNotValid : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.StatusNotValid;

    public RoadSegmentChangeGeometryStatusNotValid()
        : base(ProblemCode.ToString())
    {
    }
}
