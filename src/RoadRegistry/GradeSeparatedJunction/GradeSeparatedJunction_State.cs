namespace RoadRegistry.GradeSeparatedJunction;

using Events;
using Newtonsoft.Json;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; }
    public RoadSegmentId LowerRoadSegmentId { get; private set; }
    public RoadSegmentId UpperRoadSegmentId { get; private set; }
    public GradeSeparatedJunctionType Type { get; private set; }

    public EventTimestamp Origin { get; }
    public EventTimestamp LastModified { get; private set; }

    public bool IsRemoved { get; private set; }

    public GradeSeparatedJunction(GradeSeparatedJunctionId id, EventTimestamp origin)
        : base(id)
    {
        GradeSeparatedJunctionId = id;
        Origin = origin;
    }

    [JsonConstructor]
    protected GradeSeparatedJunction(
        int gradeSeparatedJunctionId,
        int lowerRoadSegmentId,
        int upperRoadSegmentId,
        string type,
        EventTimestamp origin,
        EventTimestamp lastModified,
        bool isRemoved
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId), origin)
    {
        LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId);
        UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId);
        Type = GradeSeparatedJunctionType.Parse(type);
        LastModified = lastModified;
        IsRemoved = isRemoved;
    }

    public static GradeSeparatedJunction Create(GradeSeparatedJunctionAdded @event)
    {
        var junction = new GradeSeparatedJunction(@event.GradeSeparatedJunctionId, @event.Provenance.ToEventTimestamp())
        {
            LowerRoadSegmentId = @event.LowerRoadSegmentId,
            UpperRoadSegmentId = @event.UpperRoadSegmentId,
            Type = @event.Type,
            LastModified = @event.Provenance.ToEventTimestamp()
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
        LastModified = @event.Provenance.ToEventTimestamp();
    }

    public void Apply(GradeSeparatedJunctionRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
        LastModified = @event.Provenance.ToEventTimestamp();
    }
}
