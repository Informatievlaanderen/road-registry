namespace RoadRegistry.RoadNode;

using Events.V2;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public RoadNodeId RoadNodeId { get; }
    public Point Geometry { get; private set; }
    public RoadNodeType Type { get; private set; }

    public EventTimestamp Origin { get; }
    public EventTimestamp LastModified { get; private set; }

    public bool IsRemoved { get; private set; }

    public RoadNode(RoadNodeId id, EventTimestamp origin)
        : base(id)
    {
        RoadNodeId = id;
        Origin = origin;
    }

    [JsonConstructor]
    protected RoadNode(
        int roadNodeId,
        Point geometry,
        string type,
        EventTimestamp origin,
        EventTimestamp lastModified,
        bool isRemoved)
        : this(new RoadNodeId(roadNodeId), origin)
    {
        Geometry = geometry;
        Type = RoadNodeType.Parse(type);
        LastModified = lastModified;
        IsRemoved = isRemoved;
    }

    public static RoadNode Create(RoadNodeAdded @event)
    {
        var roadNode = new RoadNode(@event.RoadNodeId, @event.Provenance.ToEventTimestamp())
        {
            Geometry = @event.Geometry.ToPoint(),
            Type = @event.Type,
            LastModified = @event.Provenance.ToEventTimestamp()
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }

    public void Apply(RoadNodeModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry?.ToPoint() ?? Geometry;
        Type = @event.Type ?? Type;
        LastModified = @event.Provenance.ToEventTimestamp();
    }

    public void Apply(RoadNodeRemoved @event)
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
