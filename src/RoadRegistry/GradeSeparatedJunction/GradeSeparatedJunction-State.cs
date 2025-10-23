namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice;
using RoadSegment.ValueObjects;

public partial class GradeSeparatedJunction
{
    public GradeSeparatedJunctionId Id { get; private set; }
    public RoadSegmentId LowerSegment { get; private set; }
    public RoadSegmentId UpperSegment { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }
}
