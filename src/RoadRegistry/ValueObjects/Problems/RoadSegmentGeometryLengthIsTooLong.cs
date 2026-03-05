namespace RoadRegistry.ValueObjects.Problems;

using System.Globalization;
using ProblemCodes;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentGeometryLengthIsTooLong : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Geometry.LengthTooLong;

    public RoadSegmentGeometryLengthIsTooLong(double tooLongSegmentLength)
        : base(ProblemCode.ToString(),
            new ProblemParameter("TooLongSegmentLength", tooLongSegmentLength.ToInvariantString())
        )
    {
    }

    public RoadSegmentGeometryLengthIsTooLong(RoadSegmentId identifier, double tooLongSegmentLength)
        : base(ProblemCode,
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("TooLongSegmentLength", tooLongSegmentLength.ToString(CultureInfo.InvariantCulture))
        )
    {
    }
}
