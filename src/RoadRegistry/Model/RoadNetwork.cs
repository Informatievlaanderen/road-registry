namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Framework;
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();

        private ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
        private ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;
        private ImmutableDictionary<PointM, RoadNodeId> _node_geometries;
        private readonly WellKnownBinaryReader _reader;

        private RoadNetwork()
        {
            _reader = new WellKnownBinaryReader();
            _nodes = ImmutableDictionary<RoadNodeId, RoadNode>.Empty;
            _segments = ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty;
            _node_geometries = ImmutableDictionary<PointM, RoadNodeId>.Empty;

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

            On<RoadNetworkChangesAccepted>(e =>
            {
                foreach (var change in e.Changes)
                {
                    if (change.RoadNodeAdded != null)
                    {
                        var id = new RoadNodeId(change.RoadNodeAdded.Id);
                        _nodes = _nodes.Add(id, new RoadNode(id));
                        _node_geometries = _node_geometries.Add(
                            _reader.ReadAs<PointM>(change.RoadNodeAdded.Geometry),
                            id
                        );
                    }
                }
            });
        }

        public void Change(IRequestedChange[] changes)
        {
            var writer = new WellKnownBinaryWriter();
            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            foreach (var change in changes)
            {
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        if (_nodes.ContainsKey(addRoadNode.Id))
                        {
                            rejectedChanges.Add(new RejectedChange
                            {
                                AddRoadNode = new Messages.AddRoadNode
                                {
                                    Id = addRoadNode.Id.ToInt32(),
                                    Type = (Messages.RoadNodeType) addRoadNode.Type.ToInt32(),
                                    Geometry = writer.Write(addRoadNode.Geometry)
                                }
                            });
                        }

                        if (_node_geometries.TryGetValue(addRoadNode.Geometry, out var conflictsWithId))
                        {
                            throw new RoadNodeGeometryTakenException(
                                addRoadNode.Id,
                                conflictsWithId,
                                addRoadNode.Geometry);
                        }


                        acceptedChanges.Add(new AcceptedChange
                        {
                            RoadNodeAdded = new RoadNodeAdded
                            {
                                Id = addRoadNode.Id.ToInt32(),
                                Type = (Messages.RoadNodeType) addRoadNode.Type.ToInt32(),
                                Geometry = writer.Write(addRoadNode.Geometry)
                            }
                        });
                        break;
                }
            }

            Apply(new RoadNetworkChangesAccepted
            {
                Changes = acceptedChanges.ToArray()
            });
        }
    }
}
