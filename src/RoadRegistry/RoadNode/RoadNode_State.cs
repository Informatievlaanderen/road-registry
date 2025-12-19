namespace RoadRegistry.RoadNode;

using Events.V2;
using Extensions;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public RoadNodeId RoadNodeId { get; }
    public Point Geometry { get; private set; }
    public RoadNodeType Type { get; private set; }

    public bool IsRemoved { get; private set; }

    public RoadNode(RoadNodeId id)
        : base(id)
    {
        RoadNodeId = id;
    }

    [JsonConstructor]
    protected RoadNode(
        int roadNodeId,
        Point geometry,
        string type,
        EventTimestamp origin,
        EventTimestamp lastModified,
        bool isRemoved)
        : this(new RoadNodeId(roadNodeId))
    {
        Geometry = geometry;
        Type = RoadNodeType.Parse(type);
        IsRemoved = isRemoved;
    }

    public static RoadNode Create(RoadNodeWasAdded @event)
    {
        var roadNode = new RoadNode(@event.RoadNodeId)
        {
            Geometry = @event.Geometry.ToGeometry(),
            Type = @event.Type
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }

    public void Apply(RoadNodeWasModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry?.ToGeometry() ?? Geometry;
        Type = @event.Type ?? Type;
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
