namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using Events;
    using Framework;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();

        private Dictionary<RoadNodeId, RoadNode> _nodes;
        private Dictionary<RoadSegmentId, RoadSegment> _segments;

        private RoadNetwork()
        {
            _nodes = new Dictionary<RoadNodeId, RoadNode>();
            _segments = new Dictionary<RoadSegmentId, RoadSegment>();

            On<ImportedRoadNode>(e =>
            {
                var id = new RoadNodeId(e.Id);
                var node = new RoadNode(id);
                _nodes.Add(id, node);
            });

            On<ImportedRoadSegment>(e =>
            {
                var id = new RoadSegmentId(e.Id);
                var start = new RoadNodeId(e.StartNodeId);
                var end = new RoadNodeId(e.EndNodeId);
                var segment = new RoadSegment(id, start, end);
                _nodes[start] = _nodes[start].ConnectWith(id);
                _nodes[end] = _nodes[end].ConnectWith(id);
                _segments.Add(id, segment);
            });
        }

        public void AddRoadNode(RoadNodeId id, RoadNodeType type, byte[] geometry)
        {
            Apply(new RoadNetworkChanged
            {
                Changeset = new[]
                {
                    new RoadNetworkChange
                    {
                        RoadNodeAdded = new RoadNodeAdded
                        {
                            Id = id.ToInt64(), Type = (Events.RoadNodeType) type.ToInt32(), Geometry = geometry
                        }
                    }
                }
            });
        }
    }
}
