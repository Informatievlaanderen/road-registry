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
    private readonly List<RoadSegmentId> _segments = [];

    public RoadNodeId RoadNodeId { get; init; }
    public Point Geometry { get; private set; }
    public RoadNodeType Type { get; private set; }
    public IReadOnlyCollection<RoadSegmentId> Segments => _segments.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyCollection<RoadNodeType> SupportedRoadNodeTypes
    {
        get
        {
            if (_segments.Count == 0) return [];
            if (_segments.Count == 1) return [RoadNodeType.MiniRoundabout, RoadNodeType.EndNode];
            if (_segments.Count == 2) return [RoadNodeType.MiniRoundabout, RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode];

            // 3 or more
            return [RoadNodeType.MiniRoundabout, RoadNodeType.RealNode];
        }
    }

    public RoadNode(RoadNodeId id)
        : base(id)
    {
        RoadNodeId = id;
    }

    [JsonConstructor]
    public RoadNode(
        int roadNodeId,
        Point geometry,
        string type,
        ICollection<int> segments)
        : this(new RoadNodeId(roadNodeId))
    {
        Geometry = geometry;
        Type = RoadNodeType.Parse(type);
        _segments = segments.Select(x => new RoadSegmentId(x)).ToList();
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
