namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice.Core;
using Changes;
using Events;
using RoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var roadNode = Create(new GradeSeparatedJunctionAdded
        {
            GradeSeparatedJunctionId = context.IdGenerator.NewGradeSeparatedJunctionId(),
            TemporaryId = change.TemporaryId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
