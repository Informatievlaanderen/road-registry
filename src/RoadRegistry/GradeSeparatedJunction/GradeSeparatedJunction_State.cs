namespace RoadRegistry.GradeSeparatedJunction;

using Events.V2;
using Newtonsoft.Json;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; }
    public RoadSegmentId LowerRoadSegmentId { get; private set; }
    public RoadSegmentId UpperRoadSegmentId { get; private set; }
    public GradeSeparatedJunctionTypeV2? Type { get; private set; }

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
        string? type,
        bool isRemoved
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId))
    {
        LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId);
        UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId);
        Type = type is not null ? GradeSeparatedJunctionTypeV2.Parse(type) : null;
        IsRemoved = isRemoved;
    }

    public static GradeSeparatedJunction CreateForMigration(
        GradeSeparatedJunctionId gradeSeparatedJunctionId,
        RoadSegmentId lowerRoadSegmentId,
        RoadSegmentId upperRoadSegmentId)
    {
        return new GradeSeparatedJunction(gradeSeparatedJunctionId, lowerRoadSegmentId, upperRoadSegmentId, null, false);
    }

    public static GradeSeparatedJunction Create(GradeSeparatedJunctionWasAdded @event)
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

    public void Apply(GradeSeparatedJunctionWasModified @event)
    {
        UncommittedEvents.Add(@event);

        LowerRoadSegmentId = @event.LowerRoadSegmentId ?? LowerRoadSegmentId;
        UpperRoadSegmentId = @event.UpperRoadSegmentId ?? UpperRoadSegmentId;
        Type = @event.Type ?? Type;
    }

    public void Apply(GradeSeparatedJunctionWasRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }

    public void Apply(GradeSeparatedJunctionWasRemovedBecauseOfMigration @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }
}
