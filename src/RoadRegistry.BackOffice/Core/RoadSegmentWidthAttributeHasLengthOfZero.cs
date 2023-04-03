namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentWidthAttributeHasLengthOfZero : Error
{
    public RoadSegmentWidthAttributeHasLengthOfZero(AttributeId attributeId, RoadSegmentPosition fromPosition, RoadSegmentPosition toPosition)
        : base(ProblemCode.RoadSegment.Width.HasLengthOfZero,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()))
    {
    }
}
