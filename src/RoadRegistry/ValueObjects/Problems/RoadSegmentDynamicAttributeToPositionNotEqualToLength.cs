namespace RoadRegistry.BackOffice.Core;

using Extensions;
using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributeToPositionNotEqualToLength : Error
{
    public RoadSegmentDynamicAttributeToPositionNotEqualToLength(
        RoadSegmentId roadSegmentId,
        string attributeName,
        RoadSegmentPosition toPosition,
        double length)
        : base(ProblemCode.RoadSegment.DynamicAttribute.ToPositionNotEqualToLength,
            new ProblemParameter("RoadSegmentId", roadSegmentId.ToString()),
            new ProblemParameter("AttributeName", attributeName),
            new ProblemParameter("ToPosition", toPosition.ToString()),
            new ProblemParameter("Length", length.ToRoundedMeasurementString()))
    {
    }
}
