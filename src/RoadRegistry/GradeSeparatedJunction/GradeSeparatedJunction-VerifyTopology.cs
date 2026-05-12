namespace RoadRegistry.GradeSeparatedJunction;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction
{
    public Problems VerifyTopology(ScopedRoadNetworkChangeContext context)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        if (!context.RoadNetwork.RoadSegments.TryGetValue(UpperRoadSegmentId, out var upperSegment) || upperSegment.IsRemoved)
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperSegmentMissing.ToString(),
                context.IdTranslator.TranslateToTemporaryId(UpperRoadSegmentId).ToRoadSegmentProblemParameters().ToArray());
        }

        if (!context.RoadNetwork.RoadSegments.TryGetValue(LowerRoadSegmentId, out var lowerSegment) || lowerSegment.IsRemoved)
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.LowerSegmentMissing.ToString(),
                context.IdTranslator.TranslateToTemporaryId(LowerRoadSegmentId).ToRoadSegmentProblemParameters().ToArray());
        }

        if (upperSegment is not null && !upperSegment.IsRemoved
            && lowerSegment is not null && !lowerSegment.IsRemoved
            && !upperSegment.Geometry.Value.Intersects(lowerSegment.Geometry.Value))
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperAndLowerDoNotIntersect.ToString(), Enumerable.Empty<ProblemParameter>()
                .Concat(context.IdTranslator.TranslateToTemporaryId(LowerRoadSegmentId).ToRoadSegmentProblemParameters("LowerWegsegment"))
                .Concat(context.IdTranslator.TranslateToTemporaryId(UpperRoadSegmentId).ToRoadSegmentProblemParameters("UpperWegsegment"))
                .ToArray());
        }

        return problems;
    }
}
