namespace RoadRegistry.GradeSeparatedJunction;

using Events.V2;
using Newtonsoft.Json;
using RoadRegistry.Extensions;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public GradeSeparatedJunctionId GradeSeparatedJunctionId { get; }
    public RoadSegmentId LowerRoadSegmentId { get; private set; }
    public RoadSegmentId UpperRoadSegmentId { get; private set; }
    public GradeSeparatedJunctionTypeV2? Type { get; private set; }
    public JunctionGeometry? Geometry { get; private set; }

    public bool IsRemoved { get; private set; }

    private readonly string? _lastSnapshotEventHash;
    public string LastEventHash => UncommittedEvents.Count > 0 ? UncommittedEvents[^1].GetHash() : _lastSnapshotEventHash ?? string.Empty;

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
        JunctionGeometry? geometry,
        bool isRemoved,
        string? lastEventHash
    )
        : this(new GradeSeparatedJunctionId(gradeSeparatedJunctionId))
    {
        LowerRoadSegmentId = new RoadSegmentId(lowerRoadSegmentId);
        UpperRoadSegmentId = new RoadSegmentId(upperRoadSegmentId);
        Type = type is not null ? GradeSeparatedJunctionTypeV2.Parse(type) : null;
        Geometry = geometry;
        IsRemoved = isRemoved;
        _lastSnapshotEventHash = lastEventHash;
    }

    public static GradeSeparatedJunction CreateForMigration(
        GradeSeparatedJunctionId gradeSeparatedJunctionId,
        RoadSegmentId lowerRoadSegmentId,
        RoadSegmentId upperRoadSegmentId,
        JunctionGeometry? geometry)
    {
        return new GradeSeparatedJunction(gradeSeparatedJunctionId, lowerRoadSegmentId, upperRoadSegmentId, null, geometry, false, null);
    }

    public static GradeSeparatedJunction Create(GradeSeparatedJunctionWasAdded @event)
    {
        var junction = new GradeSeparatedJunction(@event.GradeSeparatedJunctionId)
        {
            LowerRoadSegmentId = @event.LowerRoadSegmentId,
            UpperRoadSegmentId = @event.UpperRoadSegmentId,
            Type = @event.Type,
            Geometry = @event.Geometry
        };
        junction.UncommittedEvents.Add(@event);
        return junction;
    }

    public void Apply(GradeSeparatedJunctionGeometryWasChanged @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry;
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
