namespace RoadRegistry.BackOffice.Core
{
    using System.Globalization;

    public class RoadSegmentSurfaceAttributeToPositionNotEqualToLength : Error
    {
        public RoadSegmentSurfaceAttributeToPositionNotEqualToLength(AttributeId attributeId, RoadSegmentPosition toPosition, double length)
            : base(nameof(RoadSegmentSurfaceAttributeToPositionNotEqualToLength),
                new ProblemParameter("AttributeId", attributeId.ToInt32().ToString()),
                new ProblemParameter("ToPosition", toPosition.ToString()),
                new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))
        {

        }
    }
}