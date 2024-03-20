namespace RoadRegistry.BackOffice.Core;

using System.Globalization;
using ProblemCodes;

public class RoadSegmentStartPointMeasureValueNotEqualToZero : Error
{
    public RoadSegmentStartPointMeasureValueNotEqualToZero(RoadSegmentId identifier, double pointX, double pointY, double measure)
        : base(ProblemCode.RoadSegment.StartPoint.MeasureValueNotEqualToZero,
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
