namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice.Core;
using BackOffice.Core.ProblemCodes;
using RoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction
{
    public Problems VerifyTopology(RoadNetworkVerifyTopologyContext context)
    {
        var problems = Problems.None;

        if (!context.RoadNetwork.RoadSegments.TryGetValue(UpperRoadSegmentId, out var upperSegment) || upperSegment.IsRemoved)
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperSegmentMissing,
                new ProblemParameter("RoadSegmentId", context.IdTranslator.TranslateToTemporaryId(UpperRoadSegmentId).ToString()));
        }

        if (!context.RoadNetwork.RoadSegments.TryGetValue(LowerRoadSegmentId, out var lowerSegment) || lowerSegment.IsRemoved)
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.LowerSegmentMissing,
                new ProblemParameter("RoadSegmentId", context.IdTranslator.TranslateToTemporaryId(LowerRoadSegmentId).ToString()));
        }

        if (upperSegment is not null
            && lowerSegment is not null
            && !upperSegment.Geometry.Intersects(lowerSegment.Geometry))
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperAndLowerDoNotIntersect,
                new ProblemParameter("LowerRoadSegmentId", context.IdTranslator.TranslateToTemporaryId(LowerRoadSegmentId).ToString()),
                new ProblemParameter("UpperRoadSegmentId", context.IdTranslator.TranslateToTemporaryId(UpperRoadSegmentId).ToString()));
        }

        return problems;
    }
}
