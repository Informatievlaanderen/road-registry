namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributeHasLengthOfZero : Error
{
    public RoadSegmentDynamicAttributeHasLengthOfZero(
        RoadSegmentId roadSegmentId,
        string attributeName,
        RoadSegmentPosition fromPosition,
        RoadSegmentPosition toPosition)
        : base(ProblemCode.RoadSegment.DynamicAttribute.HasLengthOfZero,
            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
            new ProblemParameter("AttributeName", attributeName),
            new ProblemParameter("FromPosition", fromPosition.ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()))
    {
    }
}
