namespace RoadRegistry.RoadSegment;

using System;
using System.Linq;
using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Events;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class RoadSegment : AggregateRootEntity
{
    public Problems VerifyWithinRoadNetwork(RoadNetworkChangeContext context)
    {
        //TODO-pr verifyafter logic
        var problems = Problems.None;
        // var originalIdOrId = change.OriginalId ?? change.TemporaryId;
        //
        // var line = change.Geometry.GetSingleLineString();
        //
        // if (change.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        // {
        //     problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);
        //
        //     return problems;
        // }
        //
        // if (context.RoadNetwork.TryGetRoadSegment(segment =>
        //         segment.Id != Id &&
        //         segment.Geometry.IsReasonablyEqualTo(change.Geometry, context.Tolerances),
        //     out var byOtherSegment))
        // {
        //     problems = problems.Add(new RoadSegmentGeometryTaken(
        //         byOtherSegment.Id
        //     ));
        // }
        //
        // if (!context.RoadNetwork.TryGetRoadNode(change.StartNodeId, out var startNode))
        // {
        //     problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        // }
        // else
        // {
        //     problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
        //     if (line.StartPoint != null && !line.StartPoint.IsReasonablyEqualTo(startNode.Geometry, context.Tolerances))
        //     {
        //         problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId));
        //     }
        // }
        //
        // if (!context.AfterView.View.Nodes.TryGetValue(change.EndNodeId, out var endNode))
        // {
        //     problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        // }
        // else
        // {
        //     problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
        //     if (line.EndPoint != null && !line.EndPoint.IsReasonablyEqualTo(endNode.Geometry, context.Tolerances))
        //     {
        //         problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry(originalIdOrId));
        //     }
        // }
        //
        // if (!problems.Any())
        // {
        //     var intersectingSegments = context.AfterView.View.CreateScopedView(change.Geometry.EnvelopeInternal)
        //         .FindIntersectingRoadSegments(this);
        //     var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
        //         .Where(intersectingSegment =>
        //             !context.AfterView.GradeSeparatedJunctions.Any(junction =>
        //                 (junction.Value.LowerSegment == Id && junction.Value.UpperSegment == intersectingSegment.Key) ||
        //                 (junction.Value.LowerSegment == intersectingSegment.Key && junction.Value.UpperSegment == Id)))
        //         .Select(i =>
        //             new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
        //                 originalIdOrId,
        //                 context.Translator.TranslateToOriginalOrTemporaryOrId(i.Key)));
        //
        //     problems = problems.AddRange(intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions);
        // }

        return problems;
    }
}
