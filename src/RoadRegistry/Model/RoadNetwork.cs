namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Framework;
    using Messages;

    public class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
        public static readonly double TooCloseDistance = 2.0;

        private RoadNodeId _maximumNodeId = new RoadNodeId(0);
        private RoadSegmentId _maximumSegmentId = new RoadSegmentId(0);
        private ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
        private ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;

        private RoadNetwork()
        {
            _nodes = ImmutableDictionary<RoadNodeId, RoadNode>.Empty;
            _segments = ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty;

            On<ImportedRoadNode>(e =>
            {
                var id = new RoadNodeId(e.Id);
                var node = new RoadNode(id, GeometryTranslator.Translate(e.Geometry));
                _nodes = _nodes.Add(id, node);
                _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
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
                _maximumSegmentId = RoadSegmentId.Max(id, _maximumSegmentId);
            });

            On<RoadNetworkChangesAccepted>(e =>
            {
                foreach (var change in e.Changes)
                {
                    if (change.RoadNodeAdded != null)
                    {
                        var id = new RoadNodeId(change.RoadNodeAdded.Id);
                        _nodes = _nodes.Add(id,
                            new RoadNode(
                                id,
                                GeometryTranslator.Translate(change.RoadNodeAdded.Geometry)
                            )
                        );
                        _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
                    }
                }
            });
        }

        public void Change(IRequestedChange[] changes)
        {
            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            foreach (var change in changes)
            {
                var reasons = RejectionReasons.None;
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
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
                                _nodes.Values.FirstOrDefault(node => node.Geometry.Distance(addRoadNode.Geometry) < TooCloseDistance);
                            if (toOtherNode != null)
                            {
                                reasons = reasons.BecauseRoadNodeTooClose(toOtherNode.Id);
                            }
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            acceptedChanges.Add(addRoadNode.Accept());
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadNode.Reject(reasons));
                        }

                        break;

                    case AddRoadSegment addRoadSegment:
                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
                        if (_segments.ContainsKey(addRoadSegment.Id))
                        {
                            reasons = reasons.BecauseRoadSegmentIdTaken();
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            acceptedChanges.Add(addRoadSegment.Accept());
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadSegment.Reject(reasons));
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

        public Func<RoadNodeId> ProvidesNextRoadNodeId()
        {
            return new NextRoadNodeIdProvider(_maximumNodeId).Next;
        }

        private class NextRoadNodeIdProvider
        {
            private RoadNodeId _current;

            public NextRoadNodeIdProvider(RoadNodeId current)
            {
                _current = current;
            }

            public RoadNodeId Next()
            {
                var next = _current.Next();
                _current = next;
                return next;
            }
        }

        public Func<RoadSegmentId> ProvidesNextRoadSegmentId()
        {
            return new NextRoadSegmentIdProvider(_maximumSegmentId).Next;
        }

        private class NextRoadSegmentIdProvider
        {
            private RoadSegmentId _current;

            public NextRoadSegmentIdProvider(RoadSegmentId current)
            {
                _current = current;
            }

            public RoadSegmentId Next()
            {
                var next = _current.Next();
                _current = next;
                return next;
            }
        }
    }
}
