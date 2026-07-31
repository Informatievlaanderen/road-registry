namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

// Attribute values can only be changed on a road segment that completed its inwinning (a V2 segment); a segment that
// has not been migrated yet carries no dynamically segmented attributes at all.
public class RoadSegmentChangeAttributesNotCompletedInwinning : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeAttributes.NotCompletedInwinning;

    public RoadSegmentChangeAttributesNotCompletedInwinning(RoadSegmentId identifier)
        : base(ProblemCode.ToString(),
            new ProblemParameter("WegsegmentId", identifier.ToInt32().ToString()))
    {
    }
}
