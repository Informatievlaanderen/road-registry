namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentSurfaceAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        : base(ProblemCode.RoadSegment.Surface.FromPositionNotEqualToZero,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
