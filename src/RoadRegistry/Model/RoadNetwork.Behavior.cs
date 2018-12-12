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

            var requestView = _view.With(requestedChanges);
            var acceptedChanges = new List<AcceptedChange>();
            var rejectedChanges = new List<RejectedChange>();
            foreach (var change in requestedChanges)
            {
                var problems = Problems.With(requestedChanges);
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                    {
//                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
//                        // but this prevents from reusing the same node id in a set of changes
//                        if (incrementalNodes.ContainsKey(addRoadNode.Id))
//                        {
//                            problems = problems.BecauseRoadNodeIdTaken();
//                        }

                        var byOtherNode =
                            requestView.Nodes.Values.FirstOrDefault(n =>
                                n.Id != addRoadNode.Id &&
                                n.Geometry.EqualsExact(addRoadNode.Geometry));
                        if (byOtherNode != null)
                        {
                            problems = problems.RoadNodeGeometryTaken(byOtherNode.Id);
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
                                problems = problems.RoadNodeTypeMismatch(RoadNodeType.FakeNode,
                                    RoadNodeType.TurningLoopNode);
                            }
                            else if (addRoadNode.Type == RoadNodeType.FakeNode)
                            {
                                var segments = node.Segments.Select(segmentId => requestView.Segments[segmentId])
                                    .ToArray();
                                var segment1 = segments[0];
                                var segment2 = segments[1];
                                if (segment1.AttributeHash.Equals(segment2.AttributeHash))
                                {
                                    problems = problems.FakeRoadNodeConnectedSegmentsDoNotDiffer(segment1.Id,
                                        segment2.Id);
                                }
                            }
                        }
                        else if (connectedSegmentCount > 2 &&
                                 !addRoadNode.Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
                        {
                            problems = problems.RoadNodeTypeMismatch(RoadNodeType.RealNode,
                                RoadNodeType.MiniRoundabout);
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
                    }
                        break;

                    case AddRoadSegment addRoadSegment:
                    {
//                        // there's no way to test this without composing the changes manually (e.g. bypassing the translator).
//                        // but this prevents from reusing the same segment id in a set of changes
//                        if (incrementalSegments.ContainsKey(addRoadSegment.Id))
//                        {
//                            problems = problems.BecauseRoadSegmentIdTaken();
//                        }

                        if (Math.Abs(addRoadSegment.Geometry.Length) <= 0.001)
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

                        RoadSegmentLaneAttribute previousLane = null;
                        foreach (var lane in addRoadSegment.Lanes)
                        {
                            if (previousLane == null)
                            {
                                if (lane.From != RoadSegmentPosition.Zero)
                                {
                                    problems =
                                        problems.RoadSegmentLaneAttributeFromPositionNotEqualToZero(lane.TemporaryId);
                                }
                            }
                            else
                            {
                                if (lane.From != previousLane.To)
                                {
                                    problems =
                                        problems.RoadSegmentLaneAttributesNotAdjacent(
                                            previousLane.TemporaryId,
                                            lane.TemporaryId);
                                }
                            }

                            previousLane = lane;
                        }

                        if (previousLane != null)
                        {
                            if (Math.Abs(Math.Abs(Convert.ToDouble(previousLane.To.ToDecimal()) - line.Length)) >
                                0.0001)
                            {
                                problems =
                                    problems.RoadSegmentLaneAttributeToPositionNotEqualToLength(
                                        previousLane.TemporaryId);
                            }
                        }

                        RoadSegmentWidthAttribute previousWidth = null;
                        foreach (var lane in addRoadSegment.Widths)
                        {
                            if (previousWidth == null)
                            {
                                if (lane.From != RoadSegmentPosition.Zero)
                                {
                                    problems =
                                        problems.RoadSegmentWidthAttributeFromPositionNotEqualToZero(lane.TemporaryId);
                                }
                            }
                            else
                            {
                                if (lane.From != previousWidth.To)
                                {
                                    problems =
                                        problems.RoadSegmentWidthAttributesNotAdjacent(
                                            previousWidth.TemporaryId,
                                            lane.TemporaryId);
                                }
                            }

                            previousWidth = lane;
                        }

                        if (previousWidth != null)
                        {
                            if (Math.Abs(Math.Abs(Convert.ToDouble(previousWidth.To.ToDecimal()) - line.Length)) >
                                0.0001)
                            {
                                problems =
                                    problems.RoadSegmentWidthAttributeToPositionNotEqualToLength(
                                        previousWidth.TemporaryId);
                            }
                        }

                        RoadSegmentSurfaceAttribute previousSurface = null;
                        foreach (var lane in addRoadSegment.Surfaces)
                        {
                            if (previousSurface == null)
                            {
                                if (lane.From != RoadSegmentPosition.Zero)
                                {
                                    problems =
                                        problems.RoadSegmentSurfaceAttributeFromPositionNotEqualToZero(
                                            lane.TemporaryId);
                                }
                            }
                            else
                            {
                                if (lane.From != previousSurface.To)
                                {
                                    problems =
                                        problems.RoadSegmentSurfaceAttributesNotAdjacent(
                                            previousSurface.TemporaryId,
                                            lane.TemporaryId);
                                }
                            }

                            previousSurface = lane;
                        }

                        if (previousSurface != null)
                        {
                            if (Math.Abs(Math.Abs(Convert.ToDouble(previousSurface.To.ToDecimal()) - line.Length)) >
                                0.0001)
                            {
                                problems =
                                    problems.RoadSegmentSurfaceAttributeToPositionNotEqualToLength(
                                        previousSurface.TemporaryId);
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
                    }
                        break;
                    case AddRoadSegmentToEuropeanRoad addRoadSegmentToEuropeanRoad:
                    {
                        if (!requestView.Segments.ContainsKey(addRoadSegmentToEuropeanRoad.SegmentId))
                        {
                            problems = problems.RoadSegmentMissing(
                                addRoadSegmentToEuropeanRoad.TemporarySegmentId ?? addRoadSegmentToEuropeanRoad.SegmentId);
                        }

                        if (problems.AreAcceptable())
                        {
                            acceptedChanges.Add(addRoadSegmentToEuropeanRoad.Accept(problems));
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadSegmentToEuropeanRoad.Reject(problems));
                        }
                    }
                        break;
                    case AddRoadSegmentToNationalRoad addRoadSegmentToNationalRoad:
                    {
                        if (!requestView.Segments.ContainsKey(addRoadSegmentToNationalRoad.SegmentId))
                        {
                            problems = problems.RoadSegmentMissing(
                                addRoadSegmentToNationalRoad.TemporarySegmentId ?? addRoadSegmentToNationalRoad.SegmentId);
                        }

                        if (problems.AreAcceptable())
                        {
                            acceptedChanges.Add(addRoadSegmentToNationalRoad.Accept(problems));
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadSegmentToNationalRoad.Reject(problems));
                        }
                    }
                        break;
                    case AddRoadSegmentToNumberedRoad addRoadSegmentToNumberedRoad:
                    {
                        if (!requestView.Segments.ContainsKey(addRoadSegmentToNumberedRoad.SegmentId))
                        {
                            problems = problems.RoadSegmentMissing(
                                addRoadSegmentToNumberedRoad.TemporarySegmentId ?? addRoadSegmentToNumberedRoad.SegmentId);
                        }

                        if (problems.AreAcceptable())
                        {
                            acceptedChanges.Add(addRoadSegmentToNumberedRoad.Accept(problems));
                        }
                        else
                        {
                            rejectedChanges.Add(addRoadSegmentToNumberedRoad.Reject(problems));
                        }
                    }
                        break;
                    case AddGradeSeparatedJunction addGradeSeparatedJunction:
                    {
                        if (!requestView.Segments.TryGetValue(addGradeSeparatedJunction.UpperSegmentId, out var upperSegment))
                        {
                            problems = problems.UpperRoadSegmentMissing();
                        }

                        if (!requestView.Segments.TryGetValue(addGradeSeparatedJunction.LowerSegmentId, out var lowerSegment))
                        {
                            problems = problems.LowerRoadSegmentMissing();
                        }

                        if (upperSegment != null && lowerSegment != null)
                        {
                            if (!upperSegment.Geometry.Intersects(lowerSegment.Geometry))
                            {
                                problems = problems.UpperAndLowerRoadSegmentDoNotIntersect();
                            }
                        }

                        if (problems.AreAcceptable())
                        {
                            acceptedChanges.Add(addGradeSeparatedJunction.Accept(problems));
                        }
                        else
                        {
                            rejectedChanges.Add(addGradeSeparatedJunction.Reject(problems));
                        }
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
