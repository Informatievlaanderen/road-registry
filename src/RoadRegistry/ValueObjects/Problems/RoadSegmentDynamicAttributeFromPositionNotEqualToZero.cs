namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributeFromPositionNotEqualToZero : Error
{
    public RoadSegmentDynamicAttributeFromPositionNotEqualToZero(
        RoadSegmentId roadSegmentId,
        string attributeName,
        RoadSegmentPosition fromPosition)
        : base(ProblemCode.RoadSegment.DynamicAttribute.FromPositionNotEqualToZero,
            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
            new ProblemParameter("AttributeName", attributeName),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
