namespace RoadRegistry.BackOffice.Core;
using Extensions;
using ProblemCodes;

public class RoadSegmentSurfaceAttributeToPositionNotEqualToLength : Error
{
    public RoadSegmentSurfaceAttributeToPositionNotEqualToLength(AttributeId attributeId, RoadSegmentPosition toPosition, double length)
        : base(ProblemCode.RoadSegment.Surface.ToPositionNotEqualToLength,
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()),
            new ProblemParameter("Length", length.ToRoundedMeasurementString()))
    {
    }
}
