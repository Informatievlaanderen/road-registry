namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Events;
    using Framework;
    using Aiv.Vbr.Shaperon;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();

        private ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
        private ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;

        private RoadNetwork()
        {
            _nodes = ImmutableDictionary<RoadNodeId, RoadNode>.Empty;
            _segments = ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty;

            On<ImportedRoadNode>(e =>
            {
                var id = new RoadNodeId(e.Id);
                var node = new RoadNode(id);
                _nodes = _nodes.Add(id, node);
            });

            On<ImportedRoadSegment>(e =>
            {
                var id = new RoadSegmentId(e.Id);
                var start = new RoadNodeId(e.StartNodeId);
                var end = new RoadNodeId(e.EndNodeId);
                _nodes = _nodes
                    .TryReplaceValue(start, node => node.ConnectWith(id))
                    .TryReplaceValue(end, node => node.ConnectWith(id));
                _segments = _segments.Add(id, new RoadSegment(id, start, end));
            });

            On<RoadNetworkChanged>(e =>
            {
                foreach (var change in e.Changeset)
                {
                    if (change.RoadNodeAdded != null)
                    {
                        var id = new RoadNodeId(change.RoadNodeAdded.Id);
                        _nodes = _nodes.Add(id, new RoadNode(id));
                    }
                }
            });
        }

        public void Change(IRoadNetworkChange[] changes)
        {
            var writer = new WellKnownBinaryWriter();
            var changed = new List<RoadNetworkChange>();
            foreach (var change in changes)
            {
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        if (_nodes.ContainsKey(addRoadNode.Id))
                        {
                            throw new RoadNodeIdTakenException(addRoadNode.Id);
                        }

                        changed.Add(new RoadNetworkChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = addRoadNode.Id.ToInt64(),
                                Type = (Shared.RoadNodeType) addRoadNode.Type.ToInt32(),
                                Geometry = writer.Write(addRoadNode.Geometry)
                            }
                        });
                        break;
                }
            }

            Apply(new RoadNetworkChanged
            {
                Changeset = changed.ToArray()
            });
        }
    }
}
