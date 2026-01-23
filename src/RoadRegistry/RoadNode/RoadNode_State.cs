namespace RoadRegistry.RoadNode;

using Events.V2;
using Newtonsoft.Json;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public RoadNodeId RoadNodeId { get; }
    public RoadNodeGeometry Geometry { get; private set; }
    public RoadNodeTypeV2 Type { get; private set; }
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
        string type,
        bool grensknoop,
        EventTimestamp origin,
        EventTimestamp lastModified,
        bool isRemoved)
        : this(new RoadNodeId(roadNodeId))
    {
        Geometry = geometry;
        Type = RoadNodeTypeV2.Parse(type);
        Grensknoop = grensknoop;
        IsRemoved = isRemoved;
    }

    public static RoadNode Create(RoadNodeWasAdded @event)
    {
        var roadNode = new RoadNode(@event.RoadNodeId)
        {
            Geometry = @event.Geometry,
            Type = @event.Type,
            Grensknoop = @event.Grensknoop
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }

    public static RoadNode Create(RoadNodeWasMigrated @event)
    {
        var roadNode = new RoadNode(@event.RoadNodeId)
        {
            Geometry = @event.Geometry,
            Type = @event.Type,
            Grensknoop = @event.Grensknoop
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }

    public void Apply(RoadNodeWasModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry ?? Geometry;
        Type = @event.Type ?? Type;
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
}
