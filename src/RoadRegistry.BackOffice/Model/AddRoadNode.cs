namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Linq;
    using NetTopologySuite.Geometries;

    public class AddRoadNode : IRequestedChange
    {
        public AddRoadNode(RoadNodeId id, RoadNodeId temporaryId, RoadNodeType type, Point geometry)
        {
            Id = id;
            TemporaryId = temporaryId;
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public RoadNodeId Id { get; }
        public RoadNodeId TemporaryId { get; }
        public RoadNodeType Type { get; }
        public Point Geometry { get; }

        public IVerifiedChange Verify(VerificationContext context)
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
                problems = problems.RoadNodeTypeMismatch(RoadNodeType.EndNode);
            }
            else if (connectedSegmentCount == 2)
            {
                if (!Type.IsAnyOf(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode))
                {
                    problems = problems.RoadNodeTypeMismatch(RoadNodeType.FakeNode, RoadNodeType.TurningLoopNode);
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
                problems = problems.RoadNodeTypeMismatch(RoadNodeType.RealNode, RoadNodeType.MiniRoundabout);
            }

            if (problems.OfType<Error>().Any())
            {
                return new RejectedChange(this, problems);
            }
            return new AcceptedChange(this, problems);
        }

        public void TranslateTo(Messages.AcceptedChange message)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            message.RoadNodeAdded = new Messages.RoadNodeAdded
            {
                Id = Id,
                TemporaryId = TemporaryId,
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

            message.AddRoadNode = new Messages.AddRoadNode
            {
                TemporaryId = TemporaryId,
                Type = Type.ToString(),
                Geometry = GeometryTranslator.Translate(Geometry)
            };
        }
    }
}
