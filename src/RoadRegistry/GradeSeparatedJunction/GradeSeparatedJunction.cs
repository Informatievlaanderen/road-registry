namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice;
using BackOffice.Core;
using RoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction : MartenAggregateRootEntity<GradeSeparatedJunctionId>
{
    public Problems VerifyTopologyAfterChanges(RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        if (!context.RoadNetwork.RoadSegments.TryGetValue(UpperRoadSegmentId, out var upperSegment))
        {
            problems = problems.Add(new UpperRoadSegmentMissing());
        }

        if (!context.RoadNetwork.RoadSegments.TryGetValue(LowerRoadSegmentId, out var lowerSegment))
        {
            problems = problems.Add(new LowerRoadSegmentMissing());
        }

        if (upperSegment is not null
            && lowerSegment is not null
            && !upperSegment.Geometry.Intersects(lowerSegment.Geometry))
        {
            problems = problems.Add(new UpperAndLowerRoadSegmentDoNotIntersect());
        }

        return problems;
    }
}
