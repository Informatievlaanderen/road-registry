namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Framework;
    using Aiv.Vbr.Shaperon;
    using Messages;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
        public static readonly double TooCloseDistance = 2.0;

        private ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
        private ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;

        private RoadNetwork()
        {
            var reader = new WellKnownBinaryReader();

            _nodes = ImmutableDictionary<RoadNodeId, RoadNode>.Empty;
            _segments = ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty;

            On<ImportedRoadNode>(e =>
            {
                var id = new RoadNodeId(e.Id);
                var node = new RoadNode(id, reader.ReadAs<PointM>(e.Geometry));
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
                        _nodes = _nodes.Add(id, new RoadNode(id, reader.ReadAs<PointM>(change.RoadNodeAdded.Geometry)));
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
                        var reasons = RejectionReasons.None;

                        if (_nodes.ContainsKey(addRoadNode.Id))
                        {
                            reasons = reasons.BecauseRoadNodeIdTaken();
                        }

                        var byOtherNode =
                            _nodes.Values.FirstOrDefault(node => node.Geometry.EqualsExact(addRoadNode.Geometry));
                        if (byOtherNode != null)
                        {
                            reasons = reasons.BecauseRoadNodeGeometryTaken(byOtherNode.Id);
                        }
                        else
                        {
                            var toOtherNode =
                                _nodes.Values.FirstOrDefault(node => node.Geometry.EqualsExact(addRoadNode.Geometry, TooCloseDistance));
                            if (toOtherNode != null)
                            {
                                reasons = reasons.BecauseRoadNodeTooClose(toOtherNode.Id);
                            }
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            acceptedChanges.Add(addRoadNode.Accept(writer));
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadNode.Reject(writer, reasons));
                        }

                        break;
                }
            }

            if (rejectedChanges.Count == 0)
            {
                Apply(new RoadNetworkChangesAccepted
                {
                    Changes = acceptedChanges.ToArray()
                });
            }
            else
            {
                Apply(new RoadNetworkChangesRejected
                {
                    Changes = rejectedChanges.ToArray()
                });
            }
        }
    }
}
