namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using Point = NetTopologySuite.Geometries.Point;

public class ModifyRoadNode : IRequestedChange
{
    public ModifyRoadNode(RoadNodeId id, RoadNodeVersion version, RoadNodeType type, Point geometry)
    {
        Id = id;
        Version = version;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
    }

    public Point Geometry { get; }
    public RoadNodeId Id { get; }
    public RoadNodeVersion Version { get; }
    public RoadNodeType Type { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadNodeModified = new RoadNodeModified
            {
                Id = Id,
                Version = Version,
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

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var problems = Problems.None;

        var byOtherNode =
            context.AfterView.Nodes.Values.FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
        if (byOtherNode != null)
            problems = problems.Add(new RoadNodeGeometryTaken(
                context.Translator.TranslateToTemporaryOrId(byOtherNode.Id)
            ));

        var node = context.AfterView.Nodes[Id];

        problems = context.AfterView.Segments.Values
            .Where(s =>
                !node.Segments.Contains(s.Id) &&
                s.Geometry.IsWithinDistance(Geometry, Distances.TooClose)
            )
            .Aggregate(
                problems,
                (current, segment) =>
                    current.Add(new RoadNodeTooClose(context.Translator.TranslateToOriginalOrTemporaryOrId(segment.Id))));

        problems = problems.AddRange(node.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

        return problems;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        var problems = Problems.None;

        if (!context.BeforeView.Nodes.ContainsKey(Id)) problems = problems.Add(new RoadNodeNotFound());

        return problems;
    }
}
