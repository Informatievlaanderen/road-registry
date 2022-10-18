namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentSurfaceAttributeHasLengthOfZero : Error
{
    public RoadSegmentSurfaceAttributeHasLengthOfZero(AttributeId attributeId, RoadSegmentPosition fromPosition, RoadSegmentPosition toPosition)
        : base(nameof(RoadSegmentSurfaceAttributeHasLengthOfZero),
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()))
    {
    }
}