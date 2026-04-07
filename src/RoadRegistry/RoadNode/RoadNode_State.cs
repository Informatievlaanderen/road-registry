namespace RoadRegistry.RoadNode;

using Events.V2;
using Newtonsoft.Json;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public RoadNodeId RoadNodeId { get; }
    public RoadNodeGeometry Geometry { get; private set; }
    public RoadNodeTypeV2? Type { get; private set; }
    public bool Grensknoop { get; private set; }

    public bool IsRemoved { get; private set; }

    public RoadNode(RoadNodeId id)
        : base(id)
    {
        RoadNodeId = id;
    }

    [JsonConstructor]
    protected RoadNode(
        int roadNodeId,
        RoadNodeGeometry geometry,
        string? type,
        bool grensknoop,
        bool isRemoved)
        : this(new RoadNodeId(roadNodeId))
    {
        Geometry = geometry;
        Type = type is not null ? RoadNodeTypeV2.Parse(type) : null;
        Grensknoop = grensknoop;
        IsRemoved = isRemoved;
    }

    public static RoadNode CreateForMigration(
        RoadNodeId roadNodeId,
        RoadNodeGeometry geometry)
    {
        return new RoadNode(roadNodeId, geometry, null, false, false);
    }

    public static RoadNode Create(RoadNodeWasAdded @event)
    {
        var roadNode = new RoadNode(@event.RoadNodeId)
        {
            Geometry = @event.Geometry,
            Type = null,
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
    }

    public void Apply(RoadNodeWasRemovedBecauseOfMigration @event)
    {
        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }
}
