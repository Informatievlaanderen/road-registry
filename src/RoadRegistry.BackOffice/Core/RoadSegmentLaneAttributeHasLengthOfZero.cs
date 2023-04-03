namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentLaneAttributeHasLengthOfZero : Error
{
    public RoadSegmentLaneAttributeHasLengthOfZero(AttributeId attributeId, RoadSegmentPosition fromPosition, RoadSegmentPosition toPosition)
        : base(ProblemCode.RoadSegment.Lane.HasLengthOfZero,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()))
    {
    }
}
