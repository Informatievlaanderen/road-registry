namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using Point = NetTopologySuite.Geometries.Point;

public class ModifyRoadNode : IRequestedChange
{
    public ModifyRoadNode(RoadNodeId id, RoadNodeVersion version, RoadNodeType? type, Point? geometry)
    {
        Id = id;
        Version = version;
        Type = type;
        Geometry = geometry;
    }

    public RoadNodeId Id { get; }
    public RoadNodeVersion Version { get; }
    public RoadNodeType? Type { get; }
    public Point? Geometry { get; }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        if (!context.BeforeView.Nodes.ContainsKey(Id))
        {
            problems = problems.Add(new RoadNodeNotFound());
        }

        return problems;
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        var node = context.AfterView.Nodes[Id];

        if (Geometry is not null)
        {
            var byOtherNode =
                context.AfterView.Nodes.Values.FirstOrDefault(n =>
                    n.Id != Id &&
                    n.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
            if (byOtherNode != null)
            {
                problems = problems.Add(new RoadNodeGeometryTaken(
                    context.Translator.TranslateToTemporaryOrId(byOtherNode.Id)
                ));
            }

            problems = context.AfterView.Segments.Values
                .Where(s =>
                    !node.Segments.Contains(s.Id) &&
                    s.Geometry.IsWithinDistance(Geometry, Distances.TooClose)
                )
                .Aggregate(
                    problems,
                    (current, segment) =>
                        current.Add(new RoadNodeTooClose(context.Translator.TranslateToOriginalOrTemporaryOrId(segment.Id))));
        }

        problems = problems.AddRange(node.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

        return VerifyAfterResult.WithAcceptedChanges(problems, warnings => TranslateTo(warnings, context));
    }

    private IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings, AfterVerificationContext context)
    {
        var node = context.AfterView.Nodes[Id];

        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadNodeModified = new RoadNodeModified
            {
                Id = node.Id,
                Version = node.Version,
                Type = node.Type.ToString(),
                Geometry = new RoadNodeGeometry
                {
                    SpatialReferenceSystemIdentifier = node.Geometry.SRID,
                    Point = new Messages.Point
                    {
                        X = node.Geometry.X,
                        Y = node.Geometry.Y
                    }
                }
            }
        };
    }

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ModifyRoadNode = new Messages.ModifyRoadNode
        {
            Id = Id,
            Type = Type?.ToString(),
            Geometry = Geometry is not null ? GeometryTranslator.Translate(Geometry) : null
        };
    }
}
