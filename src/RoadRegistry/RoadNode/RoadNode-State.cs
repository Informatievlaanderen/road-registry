namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using System.Linq;
using BackOffice;
using Events;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadSegment.ValueObjects;

public partial class RoadNode
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
        var roadNode = new RoadNode(@event.Id)
        {
            Geometry = @event.Geometry.AsPoint(),
            Type = @event.Type
            //LastEventHash = @event.GetHash();
        };
        roadNode.UncommittedEvents.Add(@event);
        return roadNode;
    }
}
