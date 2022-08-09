namespace RoadRegistry.BackOffice.Core;

using System.Globalization;

public class RoadSegmentPointMeasureValueDoesNotIncrease : Error
{
    public RoadSegmentPointMeasureValueDoesNotIncrease(double pointX, double pointY, double measure, double previousMeasure)
        : base(nameof(RoadSegmentPointMeasureValueDoesNotIncrease),
            new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)),
            new ProblemParameter("PreviousMeasure", previousMeasure.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
