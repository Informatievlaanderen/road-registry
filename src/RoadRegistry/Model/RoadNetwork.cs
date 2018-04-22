namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using Aiv.Vbr.AggregateSource;
    using Events;

    public class RoadNetwork : AggregateRootEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
        
        private Dictionary<RoadNodeId, RoadNode> _nodes;
        private Dictionary<RoadSegmentId, RoadSegment> _segments;

        private RoadNetwork()
        {
            _nodes = new Dictionary<RoadNodeId, RoadNode>();
            _segments = new Dictionary<RoadSegmentId, RoadSegment>();

            Register<ImportedRoadNode>(e =>
            {
                var id = new RoadNodeId(e.Id);
                var node = new RoadNode(id);
                _nodes.Add(id, node);
            });

            Register<ImportedRoadSegment>(e =>
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
    }
}
