namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Framework;
    using Messages;

    public partial class RoadNetwork
    {
        public void Change(IReadOnlyCollection<IRequestedChange> changes)
        {
            var allNodes = _accepted_nodes;
            var allSegments = _accepted_segments;
            var incrementalNodes = _accepted_nodes;
            var incrementalSegments = _accepted_segments;
            foreach (var change in changes)
            {
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        if (!allNodes.ContainsKey(addRoadNode.Id))
                        {
                            allNodes = allNodes.Add(
                                addRoadNode.Id,
                                new RoadNode(addRoadNode.Id, addRoadNode.Geometry)
                            );
                        }

                        break;
                    case AddRoadSegment addRoadSegment:
                        allNodes = allNodes
                            .TryReplaceValue(addRoadSegment.StartNode, node => node.ConnectWith(addRoadSegment.Id))
                            .TryReplaceValue(addRoadSegment.EndNode, node => node.ConnectWith(addRoadSegment.Id));
                        if (!allSegments.ContainsKey(addRoadSegment.Id))
                        {
                            allSegments = allSegments.Add(
                                addRoadSegment.Id,
                                new RoadSegment(
                                    addRoadSegment.Id,
                                    addRoadSegment.Geometry,
                                    addRoadSegment.StartNode,
                                    addRoadSegment.EndNode)
                            );
                        }

                        break;
                }
            }

            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            foreach (var change in changes)
            {
                var reasons = RejectionReasons.None;
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
                        // but this prevents from reusing the same node id in a set of changes
                        if (incrementalNodes.ContainsKey(addRoadNode.Id))
                        {
                            reasons = reasons.BecauseRoadNodeIdTaken();
                        }

                        var byOtherNode =
                            allNodes.Values.FirstOrDefault(node =>
                                node.Id != addRoadNode.Id && node.Geometry.EqualsExact(addRoadNode.Geometry));
                        if (byOtherNode != null)
                        {
                            reasons = reasons.BecauseRoadNodeGeometryTaken(byOtherNode.Id);
                        }
                        else
                        {
                            var toOtherNode =
                                allNodes.Values.FirstOrDefault(node =>
                                    node.Id != addRoadNode.Id &&
                                    node.Geometry.Distance(addRoadNode.Geometry) < TooCloseDistance);
                            if (toOtherNode != null)
                            {
                                reasons = reasons.BecauseRoadNodeTooClose(toOtherNode.Id);
                            }
                        }

                        if (allNodes[addRoadNode.Id].Segments.Count == 0)
                        {
                            reasons = reasons.BecauseRoadNodeNotConnectedToAnySegment();
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            var node = allNodes[addRoadNode.Id];
                            incrementalNodes = incrementalNodes.Add(node.Id, node);
                            acceptedChanges.Add(addRoadNode.Accept());
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadNode.Reject(reasons));
                        }

                        break;

                    case AddRoadSegment addRoadSegment:
                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
                        // but this prevents from reusing the same segment id in a set of changes
                        if (incrementalSegments.ContainsKey(addRoadSegment.Id))
                        {
                            reasons = reasons.BecauseRoadSegmentIdTaken();
                        }

                        var byOtherSegment =
                            allSegments.Values.FirstOrDefault(segment =>
                                segment.Id != addRoadSegment.Id &&
                                segment.Geometry.EqualsExact(addRoadSegment.Geometry));
                        if (byOtherSegment != null)
                        {
                            reasons = reasons.BecauseRoadSegmentGeometryTaken(byOtherSegment.Id);
                        }

                        if (!allNodes.ContainsKey(addRoadSegment.StartNode))
                        {
                            reasons = reasons.BecauseRoadSegmentStartNodeMissing();
                        }

                        if (!allNodes.ContainsKey(addRoadSegment.EndNode))
                        {
                            reasons = reasons.BecauseRoadSegmentEndNodeMissing();
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            var segment = allSegments[addRoadSegment.Id];
                            incrementalSegments = incrementalSegments.Add(segment.Id, segment);
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

        public Func<AttributeId> ProvidesNextEuropeanRoadAttributeId()
        {
            return new NextAttributeIdProvider(_maximumEuropeanRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextNationalRoadAttributeId()
        {
            return new NextAttributeIdProvider(_maximumNationalRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextNumberedRoadAttributeId()
        {
            return new NextAttributeIdProvider(_maximumNumberedRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextLaneAttributeId()
        {
            return new NextAttributeIdProvider(_maximumLaneAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextWidthAttributeId()
        {
            return new NextAttributeIdProvider(_maximumWidthAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextSurfaceAttributeId()
        {
            return new NextAttributeIdProvider(_maximumSurfaceAttributeId).Next;
        }

        private class NextAttributeIdProvider
        {
            private AttributeId _current;

            public NextAttributeIdProvider(AttributeId current)
            {
                _current = current;
            }

            public AttributeId Next()
            {
                var next = _current.Next();
                _current = next;
                return next;
            }
        }
    }
}
