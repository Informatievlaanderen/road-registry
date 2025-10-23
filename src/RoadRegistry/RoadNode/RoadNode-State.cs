namespace RoadRegistry.RoadNode;

using System.Collections.Generic;
using BackOffice;
using NetTopologySuite.Geometries;
using RoadSegment.ValueObjects;

public partial class RoadNode
{
    private readonly List<RoadSegmentId> _segments = [];

    public RoadNodeId Id { get; }
    public Point Geometry { get; }
    public RoadNodeType Type { get; }

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
}
