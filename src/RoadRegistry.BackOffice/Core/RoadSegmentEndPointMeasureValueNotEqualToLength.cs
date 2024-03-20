namespace RoadRegistry.BackOffice.Core;

using System.Globalization;
using ProblemCodes;

public class RoadSegmentEndPointMeasureValueNotEqualToLength : Error
{
    public RoadSegmentEndPointMeasureValueNotEqualToLength(RoadSegmentId identifier, double pointX, double pointY, double measure, double length)
        : base(ProblemCode.RoadSegment.EndPoint.MeasureValueNotEqualToLength,
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Length", length.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
