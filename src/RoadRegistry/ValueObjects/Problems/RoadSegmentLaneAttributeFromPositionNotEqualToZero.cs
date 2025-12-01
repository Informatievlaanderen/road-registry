namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class RoadSegmentLaneAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentLaneAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        : base(ProblemCode.RoadSegment.Lane.FromPositionNotEqualToZero,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
