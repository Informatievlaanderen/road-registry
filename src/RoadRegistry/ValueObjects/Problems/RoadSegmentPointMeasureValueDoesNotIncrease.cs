namespace RoadRegistry.BackOffice.Core;

using System.Globalization;
using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentPointMeasureValueDoesNotIncrease : Error
{
    public RoadSegmentPointMeasureValueDoesNotIncrease(RoadSegmentId identifier, double pointX, double pointY, double measure, double previousMeasure)
        : base(ProblemCode.RoadSegment.Point.MeasureValueDoesNotIncrease,
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PreviousMeasure", previousMeasure.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
