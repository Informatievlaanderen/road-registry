namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentWidthAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentWidthAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        : base(ProblemCode.RoadSegment.Width.FromPositionNotEqualToZero,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
