namespace RoadRegistry.BackOffice.Core
{
    using System.Globalization;

    public class RoadSegmentStartPointMeasureValueNotEqualToZero : Error
    {
        public RoadSegmentStartPointMeasureValueNotEqualToZero(double pointX, double pointY, double measure)
            : base(nameof(RoadSegmentStartPointMeasureValueNotEqualToZero),
                new ProblemParameter("PointX", pointX.ToString(CultureInfo.InvariantCulture)),
                new ProblemParameter("PointY", pointY.ToString(CultureInfo.InvariantCulture)),
                new ProblemParameter("Measure", measure.ToString(CultureInfo.InvariantCulture)))
        {
            
        }
    }
}