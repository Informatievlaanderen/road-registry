namespace RoadRegistry.BackOffice.Core;

using System.Globalization;
using ProblemCodes;

public class RoadSegmentPointMeasureValueOutOfRange : Error
{
    public RoadSegmentPointMeasureValueOutOfRange(RoadSegmentId identifier, double pointX, double pointY, double measure, double measureLowerBoundary, double measureUpperBoundary)
        : base(ProblemCode.RoadSegment.Point.MeasureValueOutOfRange,
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("MeasureLowerBoundary", measureLowerBoundary.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("MeasureUpperBoundary", measureUpperBoundary.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
