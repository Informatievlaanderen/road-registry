namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice.Core;
using Events;
using RoadNetwork.Changes;
using RoadNetwork.ValueObjects;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, RoadNetworkChangeContext context)
    {
        var problems = Problems.None;

        var roadNode = Create(new GradeSeparatedJunctionAdded
        {
            Id = context.IdGenerator.NewGradeSeparatedJunctionId(),
            TemporaryId = change.TemporaryId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
