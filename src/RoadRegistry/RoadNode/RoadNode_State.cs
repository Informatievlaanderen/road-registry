namespace RoadRegistry.RoadNode;

using Events.V2;
using Newtonsoft.Json;
using RoadRegistry.Extensions;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public RoadNodeId RoadNodeId { get; }
    public RoadNodeGeometry Geometry { get; private set; }
    public RoadNodeTypeV2? Type { get; private set; }
    public bool Grensknoop { get; private set; }

    public bool IsRemoved { get; private set; }

    // Which of the two removals put it there. Only an inwinning's removal of a road node can be corrected again
    // (see RoadNodeRemovalWasCorrectedBecauseOfMigration), and a road node merged away by an editing action or by
    // the merge of the two road segments meeting at it is removed for good - that one raises RoadNodeWasRemoved.
    public bool IsRemovedBecauseOfMigration { get; private set; }

    private readonly string? _lastSnapshotEventHash;
    public string LastEventHash => UncommittedEvents.Count > 0 ? UncommittedEvents[^1].GetHash() : _lastSnapshotEventHash ?? string.Empty;

    public bool HasMigrated() => Type is not null;

    public RoadNode(RoadNodeId id, IEventOrdinalProvider ordinalProvider)
        : base(id, ordinalProvider)
    {
        RoadNodeId = id;
    }

    [JsonConstructor]
    protected RoadNode(
        int roadNodeId,
        RoadNodeGeometry geometry,
        string? type,
        bool grensknoop,
        bool isRemoved,
        bool isRemovedBecauseOfMigration,
        string? lastEventHash)
        : this(new RoadNodeId(roadNodeId), EventOrdinalProvider.None)
    {
        Geometry = geometry;
        Type = type is not null ? RoadNodeTypeV2.Parse(type) : null;
        Grensknoop = grensknoop;
        IsRemoved = isRemoved;
        IsRemovedBecauseOfMigration = isRemovedBecauseOfMigration;
        _lastSnapshotEventHash = lastEventHash;
    }

    public static RoadNode CreateForMigration(
        RoadNodeId roadNodeId,
        RoadNodeGeometry geometry)
    {
        return new RoadNode(roadNodeId, geometry, null, false, false, false, null);
    }

    public static RoadNode Create(RoadNodeWasAdded @event)
    {
        return CreateWithProvider(@event, EventOrdinalProvider.None);
    }

    // Deliberately not named "Create": Marten's snapshot aggregation discovers static Create methods and cannot
    // resolve the IEventOrdinalProvider parameter. Used by the domain Add path to stamp the created event.
    private static RoadNode CreateWithProvider(RoadNodeWasAdded @event, IEventOrdinalProvider ordinalProvider)
    {
        var roadNode = new RoadNode(@event.RoadNodeId, ordinalProvider)
        {
            Geometry = @event.Geometry,
            Type = @event.Type,
            Grensknoop = @event.Grensknoop
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }

    public void Apply(RoadNodeWasMigrated @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry;
        Grensknoop = @event.Grensknoop;
    }

    public void Apply(RoadNodeTypeWasChanged @event)
    {
        UncommittedEvents.Add(@event);

        Type = @event.Type;
    }

    public void Apply(RoadNodeWasModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry ?? Geometry;
        Grensknoop = @event.Grensknoop ?? Grensknoop;
    }

    public void Apply(RoadNodeWasRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
        IsRemovedBecauseOfMigration = false;
    }

    public void Apply(RoadNodeWasRemovedBecauseOfMigration @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = true;
        IsRemovedBecauseOfMigration = true;
    }

    public void Apply(RoadNodeRemovalWasCorrectedBecauseOfMigration @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = false;
        IsRemovedBecauseOfMigration = false;

        // The road node is collected again right after this, and dropping the type is what makes that happen: the
        // migration asks HasMigrated() whether the road node still has collecting to do. So a RoadNodeWasMigrated
        // follows with the geometry and the grensknoop, and the topology verification - which only raises the event
        // when the type differs from what it already was - adds the type back. Between them the projections that had
        // to delete their row have everything to build it again.
        Type = null;
    }
}
