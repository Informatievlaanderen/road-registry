namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentWidthAttributeHasLengthOfZero : Error
{
    public RoadSegmentWidthAttributeHasLengthOfZero(AttributeId attributeId, RoadSegmentPosition fromPosition, RoadSegmentPosition toPosition)
        : base(nameof(RoadSegmentWidthAttributeHasLengthOfZero),
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()))
    {
    }
}
