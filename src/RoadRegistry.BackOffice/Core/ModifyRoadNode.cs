namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class ModifyRoadNode : IRequestedChange
    {
        public ModifyRoadNode(RoadNodeId id, RoadNodeType type, Point geometry)
        {
            Id = id;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public RoadNodeId Id { get; }
        public RoadNodeType Type { get; }
        public Point Geometry { get; }

        public Problems VerifyBefore(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var problems = Problems.None;

            if (!context.View.Nodes.ContainsKey(Id))
            {
                problems = problems.RoadNodeNotFound();
            }

            return problems;
        }

        public Problems VerifyAfter(VerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var problems = Problems.None;

            var byOtherNode =
                context.View.Nodes.Values.FirstOrDefault(n =>
                    n.Id != Id &&
                    n.Geometry.EqualsExact(Geometry));
            if (byOtherNode != null)
            {
                problems = problems.RoadNodeGeometryTaken(
                    context.Translator.TranslateToTemporaryOrId(byOtherNode.Id)
                );
            }

            var node = context.View.Nodes[Id];

            problems = context.View.Segments.Values
                .Where(s =>
                    !node.Segments.Contains(s.Id) &&
                    s.Geometry.IsWithinDistance(Geometry, VerificationContext.TooCloseDistance)
                )
                .Aggregate(
                    problems,
                    (current, segment) =>
                        current.RoadNodeTooClose(context.Translator.TranslateToTemporaryOrId(segment.Id)));

            var connectedSegmentCount = node.Segments.Count;
            if (connectedSegmentCount == 0)
            {
                problems = problems.RoadNodeNotConnectedToAnySegment();
            }
            else if (connectedSegmentCount == 1 && Type != RoadNodeType.EndNode)
            {
                problems = problems.RoadNodeTypeMismatch(connectedSegmentCount, Type, new[] {RoadNodeType.EndNode});
            }
            else if (connectedSegmentCount == 2)
            {
                if (!Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
                {
                    problems = problems.RoadNodeTypeMismatch(connectedSegmentCount, Type,
                        new[] {RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode});
                }
                else if (Type == RoadNodeType.FakeNode)
                {
                    var segments = node.Segments.Select(segmentId => context.View.Segments[segmentId])
                        .ToArray();
                    var segment1 = segments[0];
                    var segment2 = segments[1];
                    if (segment1.AttributeHash.Equals(segment2.AttributeHash))
                    {
                        problems = problems.FakeRoadNodeConnectedSegmentsDoNotDiffer(
                            context.Translator.TranslateToTemporaryOrId(segment1.Id),
                            context.Translator.TranslateToTemporaryOrId(segment2.Id)
                        );
                    }
                }
            }
            else if (connectedSegmentCount > 2 && !Type.IsAnyOf(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout))
            {
                problems = problems.RoadNodeTypeMismatch(connectedSegmentCount, Type,
                    new[] {RoadNodeType.RealNode, RoadNodeType.MiniRoundabout});
            }

            return problems;
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadNodeModified = new Messages.RoadNodeModified
            {
                Id = Id,
                Type = Type.ToString(),
                Geometry = new Messages.RoadNodeGeometry
                {
                    SpatialReferenceSystemIdentifier = Geometry.SRID,
                    Point = new Messages.Point
                    {
                        X = Geometry.X,
                        Y = Geometry.Y
                    }
                }
            };
        }

        public void TranslateTo(Messages.RejectedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.ModifyRoadNode = new Messages.ModifyRoadNode
            {
                Id = Id,
                Type = Type.ToString(),
                Geometry = GeometryTranslator.Translate(Geometry)
            };
        }
    }
}
