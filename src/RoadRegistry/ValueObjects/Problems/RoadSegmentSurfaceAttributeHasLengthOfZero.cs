namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentSurfaceAttributeHasLengthOfZero : Error
{
    public RoadSegmentSurfaceAttributeHasLengthOfZero(AttributeId attributeId, RoadSegmentPosition fromPosition, RoadSegmentPosition toPosition)
        : base(ProblemCode.RoadSegment.Surface.HasLengthOfZero,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()))
    {
    }
}
