namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributesNotAdjacent : Error
{
    public RoadSegmentDynamicAttributesNotAdjacent(
        RoadSegmentId roadSegmentId,
        string attributeName,
        RoadSegmentPosition toPosition,
        RoadSegmentPosition fromPosition)
        : base(ProblemCode.RoadSegment.DynamicAttribute.NotAdjacent,
            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
            new ProblemParameter("AttributeName", attributeName),
            new ProblemParameter("ToPosition", toPosition.ToString()),
            new ProblemParameter("FromPosition", fromPosition.ToString()))
    {
    }
}
