namespace RoadRegistry.ValueObjects.Problems;

using Extensions;
using ProblemCodes;

public class RoadSegmentLaneAttributeToPositionNotEqualToLength : Error
{
    public RoadSegmentLaneAttributeToPositionNotEqualToLength(AttributeId attributeId,
        RoadSegmentPosition toPosition, double length)
        : base(ProblemCode.RoadSegment.Lane.ToPositionNotEqualToLength,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()),
            new ProblemParameter("Length", length.ToRoundedMeasurementString()))
    {
    }
}
