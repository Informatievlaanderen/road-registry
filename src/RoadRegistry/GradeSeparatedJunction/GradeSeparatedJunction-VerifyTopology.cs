namespace RoadRegistry.GradeSeparatedJunction;

using System.Linq;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction
{
    public Problems VerifyTopology(ScopedRoadNetworkContext context)
    {
        var problems = Problems.For(GradeSeparatedJunctionId);

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
            && !upperSegment.Geometry.Value.Intersects(lowerSegment.Geometry.Value))
        {
            problems += new Error(ProblemCode.GradeSeparatedJunction.UpperAndLowerDoNotIntersect,
                new ProblemParameter("LowerRoadSegmentId", context.IdTranslator.TranslateToTemporaryId(LowerRoadSegmentId).ToString()),
                new ProblemParameter("UpperRoadSegmentId", context.IdTranslator.TranslateToTemporaryId(UpperRoadSegmentId).ToString()));
        }

        var otherIdenticalJunctions = context.RoadNetwork.GradeSeparatedJunctions.Values
            .Where(x => !x.IsRemoved && x.GradeSeparatedJunctionId != GradeSeparatedJunctionId)
            .Where(x =>
                (x.LowerRoadSegmentId == LowerRoadSegmentId && x.UpperRoadSegmentId == UpperRoadSegmentId)
                ||
                (x.LowerRoadSegmentId == UpperRoadSegmentId && x.UpperRoadSegmentId == LowerRoadSegmentId)
            )
            .ToList();
        foreach (var otherIdenticalJunction in otherIdenticalJunctions)
        {
            problems += new GradeSeparatedJunctionNotUnique(GradeSeparatedJunctionId, otherIdenticalJunction.GradeSeparatedJunctionId);
        }

        return problems;
    }
}
