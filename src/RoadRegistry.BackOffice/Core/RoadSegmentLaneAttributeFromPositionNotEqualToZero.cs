namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentLaneAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentLaneAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        : base(nameof(RoadSegmentLaneAttributeFromPositionNotEqualToZero),
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}