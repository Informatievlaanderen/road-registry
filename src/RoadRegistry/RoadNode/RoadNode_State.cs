namespace RoadRegistry.RoadNode;

using BackOffice;
using Events;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

public partial class RoadNode : MartenAggregateRootEntity<RoadNodeId>
{
    public RoadNodeId RoadNodeId { get; }
    public Point Geometry { get; private set; }
    public RoadNodeType Type { get; private set; }

    [JsonIgnore]
    public bool IsRemoved { get; private set; }

    public RoadNode(RoadNodeId id)
        : base(id)
    {
        RoadNodeId = id;
    }

    [JsonConstructor]
    public RoadNode(
        int roadNodeId,
        Point geometry,
        string type)
        : this(new RoadNodeId(roadNodeId))
    {
        Geometry = geometry;
        Type = RoadNodeType.Parse(type);
    }

    public static RoadNode Create(RoadNodeAdded @event)
    {
        var roadNode = new RoadNode(@event.RoadNodeId)
        {
            Geometry = @event.Geometry.ToPoint(),
            Type = @event.Type
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }

    public void Apply(RoadNodeModified @event)
    {
        UncommittedEvents.Add(@event);

        Geometry = @event.Geometry?.ToPoint() ?? Geometry;
        Type = @event.Type ?? Type;
    }

    public void Apply(RoadNodeRemoved @event)
    {
        if (IsRemoved)
        {
            return;
        }

        UncommittedEvents.Add(@event);

        IsRemoved = true;
    }
}
