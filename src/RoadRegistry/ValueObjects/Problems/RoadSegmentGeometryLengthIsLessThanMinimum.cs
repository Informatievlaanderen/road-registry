namespace RoadRegistry.BackOffice.Core;

using System.Globalization;
using ProblemCodes;
using RoadSegment.ValueObjects;

public class RoadSegmentGeometryLengthIsLessThanMinimum : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Geometry.LengthLessThanMinimum;

    public RoadSegmentGeometryLengthIsLessThanMinimum(double minimum)
        : base(ProblemCode,
            new ProblemParameter("Minimum", minimum.ToString(CultureInfo.InvariantCulture))
        )
    {
    }

    public RoadSegmentGeometryLengthIsLessThanMinimum(RoadSegmentId identifier, double minimum)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("Minimum", minimum.ToString(CultureInfo.InvariantCulture))
        )
    {
    }
}
