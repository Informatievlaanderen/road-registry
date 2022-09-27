namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentSurfaceAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(AttributeId attributeId, RoadSegmentPosition fromPosition)
        : base(nameof(RoadSegmentSurfaceAttributeFromPositionNotEqualToZero),
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
