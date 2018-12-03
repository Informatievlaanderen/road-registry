namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Messages;

    public partial class RoadNetwork
    {
        public void Change(IReadOnlyCollection<IRequestedChange> changes)
        {
            var allNodes = _acceptedNodes;
            var allSegments = _acceptedSegments;
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
                            .TryReplaceValue(addRoadSegment.StartNodeId, node => node.ConnectWith(addRoadSegment.Id))
                            .TryReplaceValue(addRoadSegment.EndNodeId, node => node.ConnectWith(addRoadSegment.Id));
                        if (!allSegments.ContainsKey(addRoadSegment.Id))
                        {
                            allSegments = allSegments.Add(
                                addRoadSegment.Id,
                                new RoadSegment(
                                    addRoadSegment.Id,
                                    addRoadSegment.Geometry,
                                    addRoadSegment.StartNodeId,
                                    addRoadSegment.EndNodeId,
                                    AttributeHash.None)
                            );
                        }

                        break;
                }
            }

            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            var incrementalNodes = _acceptedNodes;
            var incrementalSegments = _acceptedSegments;
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
                            allNodes.Values.FirstOrDefault(n =>
                                n.Id != addRoadNode.Id &&
                                n.Geometry.EqualsExact(addRoadNode.Geometry));
                        if (byOtherNode != null)
                        {
                            reasons = reasons.BecauseRoadNodeGeometryTaken(byOtherNode.Id);
                        }
                        else
                        {
                            var toOtherNode =
                                allNodes.Values.FirstOrDefault(n =>
                                    n.Id != addRoadNode.Id &&
                                    n.Geometry.IsWithinDistance(addRoadNode.Geometry, TooCloseDistance));
                            if (toOtherNode != null)
                            {
                                reasons = reasons.BecauseRoadNodeTooClose(toOtherNode.Id);
                            }
                        }

                        var node = allNodes[addRoadNode.Id];
                        var connectedSegmentCount = node.Segments.Count;
                        if (connectedSegmentCount == 0)
                        {
                            reasons = reasons.BecauseRoadNodeNotConnectedToAnySegment();
                        }
                        else if (connectedSegmentCount == 1 && addRoadNode.Type != RoadNodeType.EndNode)
                        {
                            reasons = reasons.BecauseRoadNodeTypeMismatch(RoadNodeType.EndNode);
                        }
                        else if (connectedSegmentCount == 2 && (addRoadNode.Type != RoadNodeType.FakeNode || addRoadNode.Type != RoadNodeType.TurningLoopNode))
                        {
                            reasons = reasons.BecauseRoadNodeTypeMismatch(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode);

                            //reasons = reasons.BecauseFakeRoadNodeConnectedSegmentsDoNotDiffer(segment1.Id, segment2.Id);
                        }
                        else if (connectedSegmentCount > 2 && (addRoadNode.Type != RoadNodeType.RealNode || addRoadNode.Type != RoadNodeType.MiniRoundabout))
                        {
                            reasons = reasons.BecauseRoadNodeTypeMismatch(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout);
                        }

                        if (reasons == RejectionReasons.None)
                        {
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

                        if (Math.Abs(addRoadSegment.Geometry.Length) <= 0.0)
                        {
                            reasons = reasons.BecauseRoadSegmentGeometryLengthIsZero();
                        }

                        var byOtherSegment =
                            allSegments.Values.FirstOrDefault(segment =>
                                segment.Id != addRoadSegment.Id &&
                                segment.Geometry.EqualsExact(addRoadSegment.Geometry));
                        if (byOtherSegment != null)
                        {
                            reasons = reasons.BecauseRoadSegmentGeometryTaken(byOtherSegment.Id);
                        }

                        var line = addRoadSegment.Geometry.Geometries
                            .OfType<NetTopologySuite.Geometries.LineString>()
                            .Single();
                        if (!allNodes.TryGetValue(addRoadSegment.StartNodeId, out var startNode))
                        {
                            reasons = reasons.BecauseRoadSegmentStartNodeMissing();
                        }
                        else
                        {
                            if (line.StartPoint != null && !line.StartPoint.EqualsExact(startNode.Geometry))
                            {
                                reasons = reasons.BecauseRoadSegmentStartPointDoesNotMatchNodeGeometry();
                            }
                        }

                        if (!allNodes.TryGetValue(addRoadSegment.EndNodeId, out var endNode))
                        {
                            reasons = reasons.BecauseRoadSegmentEndNodeMissing();
                        }
                        else
                        {
                            if (line.EndPoint != null && !line.EndPoint.EqualsExact(endNode.Geometry))
                            {
                                reasons = reasons.BecauseRoadSegmentEndPointDoesNotMatchNodeGeometry();
                            }
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
