namespace RoadRegistry.GradeSeparatedJunction;

using System.Text.Json.Serialization;
using BackOffice;
using RoadSegment.ValueObjects;

public partial class GradeSeparatedJunction
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; init; }
    public RoadSegmentId LowerSegment { get; private set; }
    public RoadSegmentId UpperSegment { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    public GradeSeparatedJunction(GradeSeparatedJunctionId id)
        : base(id)
    {
        GradeSeparatedJunctionId = id;
    }

    [JsonConstructor]
    public GradeSeparatedJunction(
        int gradeSeparatedJunctionId,
        int lowerSegment,
        int upperSegment,
        string type
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId))
    {
        LowerSegment = new RoadSegmentId(lowerSegment);
        UpperSegment = new RoadSegmentId(upperSegment);
        Type = GradeSeparatedJunctionType.Parse(type);
    }

    public static GradeSeparatedJunction Create(object @event) //GradeSeparatedJunctionAdded
    {
        return new GradeSeparatedJunction(new GradeSeparatedJunctionId(0)) //@event.Id
        {
            //RoadNodeId = @event.Id,

            //LastEventHash = @event.GetHash();
        };
    }
}
