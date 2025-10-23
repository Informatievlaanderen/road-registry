namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using NetTopologySuite.Geometries;
using Point = NetTopologySuite.Geometries.Point;

public class AddRoadNode : IRequestedChange
{
    public AddRoadNode(RoadNodeId id, RoadNodeId temporaryId,
        RoadNodeId? originalId, RoadNodeType type, Point geometry)
    {
        Id = id;
        TemporaryId = temporaryId;
        OriginalId = originalId;
        Type = type.ThrowIfNull();
        Geometry = geometry.ThrowIfNull();
    }

    public Point Geometry { get; }
    public RoadNodeId Id { get; }
    public RoadNodeId TemporaryId { get; }
    public RoadNodeId? OriginalId { get; }
    public RoadNodeType Type { get; }

    public IEnumerable<Messages.AcceptedChange> TranslateTo(BackOffice.Messages.Problem[] warnings)
    {
        yield return new Messages.AcceptedChange
        {
            Problems = warnings,
            RoadNodeAdded = new RoadNodeAdded
            {
                Id = Id,
                Version = RoadNodeVersion.Initial,
                TemporaryId = TemporaryId,
                OriginalId = OriginalId,
                Type = Type,
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

    public void TranslateToRejectedChange(Messages.RejectedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.AddRoadNode = new Messages.AddRoadNode
        {
            TemporaryId = TemporaryId,
            OriginalId = OriginalId,
            Type = Type,
            Geometry = GeometryTranslator.Translate(Geometry)
        };
    }

    public VerifyAfterResult VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        var byOtherNode =
            context.AfterView.Nodes.Values.FirstOrDefault(n =>
                n.Id != Id &&
                n.Geometry.IsReasonablyEqualTo(Geometry, context.Tolerances));
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
                    current.Add(new RoadNodeTooClose(context.Translator.TranslateToOriginalOrTemporaryOrId(segment.Id))));

        problems = problems.AddRange(node.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));

        return VerifyAfterResult.WithAcceptedChanges(problems, TranslateTo);
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));
        return Problems.None;
    }
}
