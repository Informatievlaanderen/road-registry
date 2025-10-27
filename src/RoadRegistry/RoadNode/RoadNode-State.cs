namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using BackOffice;
using NetTopologySuite.Geometries;
using RoadSegment.ValueObjects;

public partial class RoadNode
{
    public string Id => RoadNodeId.ToString(); // Required for MartenDb

    private readonly List<RoadSegmentId> _segments = [];

    public RoadNodeId RoadNodeId { get; }
    public Point Geometry { get; private set; }
    public RoadNodeType Type { get; private set; }

    public IReadOnlyCollection<RoadSegmentId> Segments => _segments.AsReadOnly();
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

    public static RoadNode Create(object @event) //RoadNodeAdded
    {
        return new RoadNode
        {
            //RoadNodeId = @event.Id,

            //LastEventHash = @event.GetHash();
        };
    }
}
