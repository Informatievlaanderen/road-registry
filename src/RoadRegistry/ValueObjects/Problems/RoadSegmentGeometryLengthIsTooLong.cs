namespace RoadRegistry.BackOffice.Core;

using System.Globalization;
using ProblemCodes;

public class RoadSegmentGeometryLengthIsTooLong : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Geometry.LengthTooLong;

    public RoadSegmentGeometryLengthIsTooLong(double tooLongSegmentLength)
        : base(ProblemCode,
            new ProblemParameter("TooLongSegmentLength", tooLongSegmentLength.ToString(CultureInfo.InvariantCulture))
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
