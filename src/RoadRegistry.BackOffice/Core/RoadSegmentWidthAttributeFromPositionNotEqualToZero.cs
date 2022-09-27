namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentWidthAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentWidthAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        : base(nameof(RoadSegmentWidthAttributeFromPositionNotEqualToZero),
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
