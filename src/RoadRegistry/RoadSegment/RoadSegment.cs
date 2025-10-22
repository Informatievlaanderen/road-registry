namespace RoadRegistry.RoadSegment;

using BackOffice.Core;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class RoadSegment
{
    // aggregate: command handler, event appliers

    public Problems Add(AddRoadSegmentChange change, RoadNetworkChangeContext context)
    {

        // verify before (guard)
        VerifyBefore()

        // generate + apply events

        // verify after

        // return events

        var problems = Problems.None;
        var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

        var line = change.Geometry.GetSingleLineString();

        if (change.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

            return VerifyAfterResult.WithAcceptedChanges(problems, warnings => TranslateTo(warnings, context));
        }

        var byOtherSegment =
            context.AfterView.Segments.Values.FirstOrDefault(segment =>
                segment.Id != Id &&
                segment.Geometry.IsReasonablyEqualTo(change.Geometry, context.Tolerances));
        if (byOtherSegment != null)
        {
            problems = problems.Add(new RoadSegmentGeometryTaken(
                context.Translator.TranslateToOriginalOrTemporaryOrId(byOtherSegment.Id)
            ));
        }

        if (!context.AfterView.View.Nodes.TryGetValue(change.StartNodeId, out var startNode))
        {
            problems = problems.Add(new RoadSegmentStartNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(startNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line.StartPoint != null && !line.StartPoint.IsReasonablyEqualTo(startNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentStartPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!context.AfterView.View.Nodes.TryGetValue(change.EndNodeId, out var endNode))
        {
            problems = problems.Add(new RoadSegmentEndNodeMissing(originalIdOrId));
        }
        else
        {
            problems = problems.AddRange(endNode.VerifyTypeMatchesConnectedSegmentCount(context.AfterView.View, context.Translator));
            if (line.EndPoint != null && !line.EndPoint.IsReasonablyEqualTo(endNode.Geometry, context.Tolerances))
            {
                problems = problems.Add(new RoadSegmentEndPointDoesNotMatchNodeGeometry(originalIdOrId));
            }
        }

        if (!problems.Any())
        {
            var intersectingSegments = context.AfterView.View.CreateScopedView(change.Geometry.EnvelopeInternal)
                .FindIntersectingRoadSegments(this);
            var intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions = intersectingSegments
                .Where(intersectingSegment =>
                    !context.AfterView.GradeSeparatedJunctions.Any(junction =>
                        (junction.Value.LowerSegment == Id && junction.Value.UpperSegment == intersectingSegment.Key) ||
                        (junction.Value.LowerSegment == intersectingSegment.Key && junction.Value.UpperSegment == Id)))
                .Select(i =>
                    new IntersectingRoadSegmentsDoNotHaveGradeSeparatedJunction(
                        originalIdOrId,
                        context.Translator.TranslateToOriginalOrTemporaryOrId(i.Key)));

            problems = problems.AddRange(intersectingRoadSegmentsDoNotHaveGradeSeparatedJunctions);
        }

        return VerifyAfterResult.WithAcceptedChanges(problems, warnings => TranslateTo(warnings, context));
    //}

        Problems VerifyBefore(BeforeVerificationContext context)
        {
            var problems = Problems.None;
            var originalIdOrId = context.Translator.TranslateToOriginalOrTemporaryOrId(Id);

            var line = change.Geometry.GetSingleLineString();

            if (change.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
            {
                problems += line.GetProblemsForRoadSegmentOutlinedGeometry(originalIdOrId, context.Tolerances);

                return problems;
            }

            problems += line.GetProblemsForRoadSegmentGeometry(originalIdOrId, context.Tolerances);
            problems += line.GetProblemsForRoadSegmentLanes(change.Lanes, context.Tolerances);
            problems += line.GetProblemsForRoadSegmentWidths(change.Widths, context.Tolerances);
            problems += line.GetProblemsForRoadSegmentSurfaces(change.Surfaces, context.Tolerances);

            return problems;
        }
    }

    public Problems Modify()
    {

    }
}
