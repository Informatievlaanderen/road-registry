namespace RoadRegistry.BackOffice.Core
{
    public class RoadSegmentLaneAttributeHasLengthOfZero : Error
    {
        public RoadSegmentLaneAttributeHasLengthOfZero(AttributeId attributeId, RoadSegmentPosition fromPosition, RoadSegmentPosition toPosition)
            : base(nameof(RoadSegmentLaneAttributeHasLengthOfZero),
                new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                new ProblemParameter("FromPosition", fromPosition.ToString()),
                new ProblemParameter("ToPosition", toPosition.ToString()))
        {
        }
    }
}
