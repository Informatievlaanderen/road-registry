namespace RoadRegistry.BackOffice.Core;

using System;
using System.Linq;
using Messages;
using Point = NetTopologySuite.Geometries.Point;

public class AddRoadNode : IRequestedChange
{
    public AddRoadNode(RoadNodeId id, RoadNodeId temporaryId, RoadNodeType type, Point geometry)
    {
        Id = id;
        TemporaryId = temporaryId;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public Point Geometry { get; }
    public RoadNodeId Id { get; }
    public RoadNodeId TemporaryId { get; }
    public RoadNodeType Type { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RoadNodeAdded = new RoadNodeAdded
        {
            Id = Id,
            Version = RoadNodeVersion.Initial,
            TemporaryId = TemporaryId,
            Type = Type.ToString(),
            Geometry = new RoadNodeGeometry
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

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var problems = Problems.None;

        var byOtherNode =
            context.AfterView.Nodes.Values.FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.EqualsWithinTolerance(Geometry, context.Tolerances.GeometryTolerance));
        if (byOtherNode != null)
            problems = problems.Add(new RoadNodeGeometryTaken(
                context.Translator.TranslateToTemporaryOrId(byOtherNode.Id)
            ));

        var node = context.AfterView.View.Nodes[Id];

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

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return Problems.None;
    }
}
