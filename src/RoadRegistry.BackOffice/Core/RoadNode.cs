namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using NetTopologySuite.Geometries;

    public class RoadNode
    {
        private readonly ImmutableHashSet<RoadSegmentId> _segments;

        public RoadNodeId Id { get; }
        public Point Geometry { get; }

        public IReadOnlyCollection<RoadSegmentId> Segments => _segments;

        public RoadNode(RoadNodeId id, Point geometry)
        {
            Id = id;
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
            _segments = ImmutableHashSet<RoadSegmentId>.Empty;
        }

        private RoadNode(RoadNodeId id, Point geometry, ImmutableHashSet<RoadSegmentId> segments)
        {
            Id = id;
            Geometry = geometry;
            _segments = segments;
        }

        public RoadNode ConnectWith(RoadSegmentId segment)
        {
            return new RoadNode(Id, Geometry, _segments.Add(segment));
        }

        public RoadNode DisconnectFrom(RoadSegmentId segment)
        {
            return new RoadNode(Id, Geometry, _segments.Remove(segment));
        }

        public IReadOnlyCollection<RoadNodeType> SupportedRoadNodeTypes
        {
            get
            {
                if (_segments.Count == 0)
                {
                    return Array.Empty<RoadNodeType>();
                }

                if (_segments.Count == 1)
                {
                    return new[] { RoadNodeType.MiniRoundabout, RoadNodeType.EndNode };
                }

                if (_segments.Count == 2)
                {
                    return new[] { RoadNodeType.MiniRoundabout, RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode };
                }

                // 3 or more
                return new[] { RoadNodeType.MiniRoundabout, RoadNodeType.RealNode };
            }
        }

        //public KeyValuePair<RoadNodeId, RoadNode> ToKeyValuePair() => new KeyValuePair<RoadNodeId, RoadNode>(Id, this);

        // public Builder ToBuilder() => new Builder(this);

        // public class Builder
        // {
        //     private readonly ImmutableHashSet<RoadSegmentId>.Builder _segments;
        //     public RoadNodeId Id { get; }

        //     public IReadOnlyCollection<RoadSegmentId> Segments => _segments;

        //     internal Builder(RoadNode node)
        //     {
        //         Id = node.Id;
        //         _segments = node._segments.ToBuilder();
        //     }

        //     public void ConnectWith(RoadSegmentId link)
        //     {
        //         _segments.Add(link);
        //     }

        //     public void DisconnectFrom(RoadSegmentId link)
        //     {
        //         _segments.Remove(link);
        //     }

        //     public RoadNode ToImmutable() => new RoadNode(Id, _segments.ToImmutable());
        // }
    }
}
