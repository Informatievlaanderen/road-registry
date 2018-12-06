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
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            //Reject(errors)/Accept(warnings)

            var requestView = _view.When(changes);
            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            foreach (var change in changes)
            {
                var reasons = RejectionReasons.None;
                switch (change)
                {
                    case AddRoadNode addRoadNode:
//                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
//                        // but this prevents from reusing the same node id in a set of changes
//                        if (incrementalNodes.ContainsKey(addRoadNode.Id))
//                        {
//                            reasons = reasons.BecauseRoadNodeIdTaken();
//                        }

                        var byOtherNode =
                            requestView.AcceptedNodes.Values.FirstOrDefault(n =>
                                n.Id != addRoadNode.Id &&
                                n.Geometry.EqualsExact(addRoadNode.Geometry));
                        if (byOtherNode != null)
                        {
                            reasons = reasons.BecauseRoadNodeGeometryTaken(byOtherNode.Id);
                        }
                        else
                        {
                            var toOtherNode =
                                requestView.AcceptedNodes.Values.FirstOrDefault(n =>
                                    n.Id != addRoadNode.Id &&
                                    n.Geometry.IsWithinDistance(addRoadNode.Geometry, TooCloseDistance));
                            if (toOtherNode != null)
                            {
                                reasons = reasons.BecauseRoadNodeTooClose(toOtherNode.Id);
                            }
                        }

                        var node = requestView.AcceptedNodes[addRoadNode.Id];
                        var connectedSegmentCount = node.Segments.Count;
                        if (connectedSegmentCount == 0)
                        {
                            reasons = reasons.BecauseRoadNodeNotConnectedToAnySegment();
                        }
                        else if (connectedSegmentCount == 1 && addRoadNode.Type != RoadNodeType.EndNode)
                        {
                            reasons = reasons.BecauseRoadNodeTypeMismatch(RoadNodeType.EndNode);
                        }
                        else if (connectedSegmentCount == 2)
                        {
                            if (!addRoadNode.Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
                            {
                                reasons = reasons.BecauseRoadNodeTypeMismatch(
                                    RoadNodeType.FakeNode,
                                    RoadNodeType.TurningLoopNode);

                            }
                            else if (addRoadNode.Type == RoadNodeType.FakeNode)
                            {
                                var segments = node.Segments.Select(segmentId => requestView.AcceptedSegments[segmentId]).ToArray();
                                var segment1 = segments[0];
                                var segment2 = segments[1];
                                if (segment1.AttributeHash.Equals(segment2.AttributeHash))
                                {
                                    reasons = reasons.BecauseFakeRoadNodeConnectedSegmentsDoNotDiffer(segment1.Id, segment2.Id);
                                }
                            }
                        }
                        else if (connectedSegmentCount > 2 && !addRoadNode.Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
                        {
                            reasons = reasons.BecauseRoadNodeTypeMismatch(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout);
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            //incrementalNodes = incrementalNodes.Add(node.Id, node);
                            acceptedChanges.Add(addRoadNode.Accept());
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadNode.Reject(reasons));
                        }

                        break;

                    case AddRoadSegment addRoadSegment:
//                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
//                        // but this prevents from reusing the same segment id in a set of changes
//                        if (incrementalSegments.ContainsKey(addRoadSegment.Id))
//                        {
//                            reasons = reasons.BecauseRoadSegmentIdTaken();
//                        }

                        if (Math.Abs(addRoadSegment.Geometry.Length) <= 0.0)
                        {
                            reasons = reasons.BecauseRoadSegmentGeometryLengthIsZero();
                        }

                        var byOtherSegment =
                            requestView.AcceptedSegments.Values.FirstOrDefault(segment =>
                                segment.Id != addRoadSegment.Id &&
                                segment.Geometry.EqualsExact(addRoadSegment.Geometry));
                        if (byOtherSegment != null)
                        {
                            reasons = reasons.BecauseRoadSegmentGeometryTaken(byOtherSegment.Id);
                        }

                        var line = addRoadSegment.Geometry.Geometries
                            .OfType<NetTopologySuite.Geometries.LineString>()
                            .Single();
                        if (!requestView.AcceptedNodes.TryGetValue(addRoadSegment.StartNodeId, out var startNode))
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

                        if (!requestView.AcceptedNodes.TryGetValue(addRoadSegment.EndNodeId, out var endNode))
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

                        if (line.SelfOverlaps())
                        {
                            reasons = reasons.BecauseRoadSegmentGeometrySelfOverlaps();
                        }
                        else if (line.SelfIntersects())
                        {
                            reasons = reasons.BecauseRoadSegmentGeometrySelfIntersects();
                        }

                        if (reasons == RejectionReasons.None)
                        {
                            var segment = requestView.AcceptedSegments[addRoadSegment.Id];
                            //incrementalSegments = incrementalSegments.Add(segment.Id, segment);
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
            return new NextRoadNodeIdProvider(_view.MaximumNodeId).Next;
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
            return new NextRoadSegmentIdProvider(_view.MaximumSegmentId).Next;
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
            return new NextAttributeIdProvider(_view.MaximumEuropeanRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextNationalRoadAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumNationalRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextNumberedRoadAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumNumberedRoadAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextLaneAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumLaneAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextWidthAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumWidthAttributeId).Next;
        }

        public Func<AttributeId> ProvidesNextSurfaceAttributeId()
        {
            return new NextAttributeIdProvider(_view.MaximumSurfaceAttributeId).Next;
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
