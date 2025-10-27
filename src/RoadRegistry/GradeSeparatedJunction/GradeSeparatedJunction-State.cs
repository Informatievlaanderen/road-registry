namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice;
using RoadSegment.ValueObjects;

public partial class GradeSeparatedJunction
{
    public string Id => GradeSeparatedJunctionId.ToString(); // Required for MartenDb

    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId LowerSegment { get; private set; }
    public RoadSegmentId UpperSegment { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    public static GradeSeparatedJunction Create(object @event) //GradeSeparatedJunctionAdded
    {
        return new GradeSeparatedJunction
        {
            //RoadNodeId = @event.Id,

            //LastEventHash = @event.GetHash();
        };
    }
}
