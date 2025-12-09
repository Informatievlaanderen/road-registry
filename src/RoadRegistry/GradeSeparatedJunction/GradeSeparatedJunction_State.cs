namespace RoadRegistry.GradeSeparatedJunction;

using Events.V2;
using Newtonsoft.Json;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; }
    public RoadSegmentId LowerRoadSegmentId { get; private set; }
    public RoadSegmentId UpperRoadSegmentId { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    public bool IsRemoved { get; private set; }

    public GradeSeparatedJunction(GradeSeparatedJunctionId id)
        : base(id)
    {
        GradeSeparatedJunctionId = id;
    }

    [JsonConstructor]
    protected GradeSeparatedJunction(
        int gradeSeparatedJunctionId,
        int lowerRoadSegmentId,
        int upperRoadSegmentId,
        string type,
        bool isRemoved
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId))
    {
        LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId);
        UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId);
        Type = GradeSeparatedJunctionType.Parse(type);
        IsRemoved = isRemoved;
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

    public void Apply(GradeSeparatedJunctionModified @event)
    {
        UncommittedEvents.Add(@event);

        LowerRoadSegmentId = @event.LowerRoadSegmentId ?? LowerRoadSegmentId;
        UpperRoadSegmentId = @event.UpperRoadSegmentId ?? UpperRoadSegmentId;
        Type = @event.Type ?? Type;
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
