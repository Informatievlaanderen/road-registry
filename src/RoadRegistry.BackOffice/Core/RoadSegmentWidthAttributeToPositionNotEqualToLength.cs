namespace RoadRegistry.BackOffice.Core;

using System.Globalization;

public class RoadSegmentWidthAttributeToPositionNotEqualToLength : Error
{
    public RoadSegmentWidthAttributeToPositionNotEqualToLength(AttributeId attributeId, RoadSegmentPosition toPosition, double length)
        : base(nameof(RoadSegmentWidthAttributeToPositionNotEqualToLength),
            new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
            new ProblemParameter("ToPosition", toPosition.ToString()),
            new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))
    {
    }
}