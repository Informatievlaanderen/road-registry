namespace RoadRegistry.BackOffice.Core;

using System.Globalization;

public class RoadSegmentEndPointMeasureValueNotEqualToLength : Error
{
    public RoadSegmentEndPointMeasureValueNotEqualToLength(double pointX, double pointY, double measure, double length)
        : base(nameof(RoadSegmentEndPointMeasureValueNotEqualToLength),
            new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
