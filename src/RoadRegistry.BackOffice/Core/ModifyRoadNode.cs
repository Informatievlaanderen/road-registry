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

        public Problems VerifyBefore(BeforeVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var problems = Problems.None;

            if (!context.BeforeView.Nodes.ContainsKey(Id))
            {
                problems = problems.Add(new RoadNodeNotFound());
            }

            return problems;
        }

        public Problems VerifyAfter(AfterVerificationContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            var problems = Problems.None;

            var byOtherNode =
                context.AfterView.Nodes.Values.FirstOrDefault(n =>
                    n.Id != Id &&
                    n.Geometry.EqualsWithinTolerance(Geometry, context.Tolerances.GeometryTolerance));
            if (byOtherNode != null)
            {
                problems = problems.Add(new RoadNodeGeometryTaken(
                    context.Translator.TranslateToTemporaryOrId(byOtherNode.Id)
                ));
            }

            var node = context.AfterView.Nodes[Id];

            problems = context.AfterView.Segments.Values
                .Where(s =>
                    !node.Segments.Contains(s.Id) &&
                    s.Geometry.IsWithinDistance(Geometry, Distances.TooClose)
                )
                .Aggregate(
                    problems,
                    (current, segment) =>
                        current.Add(new RoadNodeTooClose(context.Translator.TranslateToTemporaryOrId(segment.Id))));

            problems = problems.AddRange(node.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

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
