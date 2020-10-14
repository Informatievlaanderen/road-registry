namespace RoadRegistry.BackOffice.Core
{
    public class RoadSegmentSurfaceAttributesNotAdjacent : Error
    {
        public RoadSegmentSurfaceAttributesNotAdjacent(AttributeId attributeId1, RoadSegmentPosition toPosition, AttributeId attributeId2, RoadSegmentPosition fromPosition)
            : base(nameof(RoadSegmentSurfaceAttributesNotAdjacent),
                new ProblemParameter("AttributeId", attributeId1.ToInt32().ToString()),
                new ProblemParameter("ToPosition", toPosition.ToString()),
                new ProblemParameter("AttributeId", attributeId2.ToInt32().ToString()),
                new ProblemParameter("FromPosition", fromPosition.ToString()))
        {

        }
    }
}