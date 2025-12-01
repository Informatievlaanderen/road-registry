namespace RoadRegistry.GradeSeparatedJunction;

using System.Text.Json.Serialization;
using Events;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; }
    public RoadSegmentId LowerRoadSegmentId { get; private set; }
    public RoadSegmentId UpperRoadSegmentId { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    [JsonIgnore]
    public bool IsRemoved { get; private set; }

    public GradeSeparatedJunction(GradeSeparatedJunctionId id)
        : base(id)
    {
        GradeSeparatedJunctionId = id;
    }

    [JsonConstructor]
    public GradeSeparatedJunction(
        int gradeSeparatedJunctionId,
        int lowerRoadSegmentId,
        int upperRoadSegmentId,
        string type
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId))
    {
        LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId);
        UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId);
        Type = GradeSeparatedJunctionType.Parse(type);
    }

    public static GradeSeparatedJunction Create(GradeSeparatedJunctionAdded @event)
    {
        var junction = new GradeSeparatedJunction(@event.GradeSeparatedJunctionId)
        {
            LowerRoadSegmentId = @event.LowerRoadSegmentId,
            UpperRoadSegmentId = @event.UpperRoadSegmentId,
            Type = @event.Type
        };
        junction.UncommittedEvents.Add(@event);
        return junction;
    }

    public void Apply(GradeSeparatedJunctionRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }
}
