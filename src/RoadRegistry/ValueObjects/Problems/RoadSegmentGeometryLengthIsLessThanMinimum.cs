namespace RoadRegistry.ValueObjects.Problems;

using System.Globalization;
using ProblemCodes;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentGeometryLengthIsLessThanMinimum : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Geometry.LengthLessThanMinimum;

    public RoadSegmentGeometryLengthIsLessThanMinimum(double minimum)
        : base(ProblemCode.ToString(),
            new ProblemParameter("Minimum", minimum.ToInvariantString())
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
