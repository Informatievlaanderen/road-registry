namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Messages;

    public partial class RoadNetwork
    {
        public void Change(IRequestedChanges requestedChanges)
        {
            //TODO: Verify there are no duplicate identifiers (will fail anyway) and report as rejection

            var requestView = _view.When(requestedChanges);
            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            foreach (var change in requestedChanges)
            {
                var problems = Problems.With(requestedChanges);
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
                            requestView.Nodes.Values.FirstOrDefault(n =>
                                n.Id != addRoadNode.Id &&
                                n.Geometry.EqualsExact(addRoadNode.Geometry));
                        if (byOtherNode != null)
                        {
                            problems = problems.RoadNodeGeometryTaken(byOtherNode.Id);
                        }
                        else
                        {
                            var toOtherNode =
                                requestView.Nodes.Values.FirstOrDefault(n =>
                                    n.Id != addRoadNode.Id &&
                                    n.Geometry.IsWithinDistance(addRoadNode.Geometry, TooCloseDistance));
                            if (toOtherNode != null)
                            {
                                problems = problems.RoadNodeTooClose(toOtherNode.Id);
                            }
                        }

                        var node = requestView.Nodes[addRoadNode.Id];
                        var connectedSegmentCount = node.Segments.Count;
                        if (connectedSegmentCount == 0)
                        {
                            problems = problems.RoadNodeNotConnectedToAnySegment();
                        }
                        else if (connectedSegmentCount == 1 && addRoadNode.Type != RoadNodeType.EndNode)
                        {
                            problems = problems.RoadNodeTypeMismatch(RoadNodeType.EndNode);
                        }
                        else if (connectedSegmentCount == 2)
                        {
                            if (!addRoadNode.Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
                            {
                                problems = problems.RoadNodeTypeMismatch(
                                    RoadNodeType.FakeNode,
                                    RoadNodeType.TurningLoopNode);

                            }
                            else if (addRoadNode.Type == RoadNodeType.FakeNode)
                            {
                                var segments = node.Segments.Select(segmentId => requestView.Segments[segmentId]).ToArray();
                                var segment1 = segments[0];
                                var segment2 = segments[1];
                                if (segment1.AttributeHash.Equals(segment2.AttributeHash))
                                {
                                    problems = problems.FakeRoadNodeConnectedSegmentsDoNotDiffer(segment1.Id, segment2.Id);
                                }
                            }
                        }
                        else if (connectedSegmentCount > 2 && !addRoadNode.Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
                        {
                            problems = problems.RoadNodeTypeMismatch(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout);
                        }

                        if (problems.AreAcceptable())
                        {
                            //incrementalNodes = incrementalNodes.Add(node.Id, node);
                            acceptedChanges.Add(addRoadNode.Accept(problems));
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadNode.Reject(problems));
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
                            problems = problems.RoadSegmentGeometryLengthIsZero();
                        }

                        var byOtherSegment =
                            requestView.Segments.Values.FirstOrDefault(segment =>
                                segment.Id != addRoadSegment.Id &&
                                segment.Geometry.EqualsExact(addRoadSegment.Geometry));
                        if (byOtherSegment != null)
                        {
                            problems = problems.RoadSegmentGeometryTaken(byOtherSegment.Id);
                        }

                        var line = addRoadSegment.Geometry.Geometries
                            .OfType<NetTopologySuite.Geometries.LineString>()
                            .Single();
                        if (!requestView.Nodes.TryGetValue(addRoadSegment.StartNodeId, out var startNode))
                        {
                            problems = problems.RoadSegmentStartNodeMissing();
                        }
                        else
                        {
                            if (line.StartPoint != null && !line.StartPoint.EqualsExact(startNode.Geometry))
                            {
                                problems = problems.RoadSegmentStartPointDoesNotMatchNodeGeometry();
                            }
                        }

                        if (!requestView.Nodes.TryGetValue(addRoadSegment.EndNodeId, out var endNode))
                        {
                            problems = problems.RoadSegmentEndNodeMissing();
                        }
                        else
                        {
                            if (line.EndPoint != null && !line.EndPoint.EqualsExact(endNode.Geometry))
                            {
                                problems = problems.RoadSegmentEndPointDoesNotMatchNodeGeometry();
                            }
                        }

                        if (line.SelfOverlaps())
                        {
                            problems = problems.RoadSegmentGeometrySelfOverlaps();
                        }
                        else if (line.SelfIntersects())
                        {
                            problems = problems.RoadSegmentGeometrySelfIntersects();
                        }

                        var position = new RoadSegmentPosition(0.0m);
                        var index = 0;
                        foreach(var attribute in addRoadSegment.Lanes)
                        {
                            if(attribute.From != position)
                            {
                                if (index == 0)
                                {
                                    problems = problems.RoadSegmentLaneAttributeFirstFromPositionNotEqualToZero();
                                }
                                else
                                {
                                    problems = problems.RoadSegmentLaneAttributeNotAdjacentToPrevious(attribute.From, position);
                                }
                            }
                            position = attribute.To;
                            index++;
                            if (index == addRoadSegment.Lanes.Count)
                            {
                                if (Math.Abs(Convert.ToDouble(attribute.To.ToDecimal()) - line.Length) > 0.0)
                                {
                                    problems = problems.RoadSegmentLaneAttributeLastToPositionNotEqualToLength(attribute.To, line.Length);
                                }
                            }
                        }

                        if (problems.AreAcceptable())
                        {
                            acceptedChanges.Add(addRoadSegment.Accept(problems));
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadSegment.Reject(problems));
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
