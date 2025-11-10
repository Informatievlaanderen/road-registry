namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice.Core;
using Changes;
using Events;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.None;

        var roadNode = Create(new GradeSeparatedJunctionAdded
        {
            GradeSeparatedJunctionId = idGenerator.NewGradeSeparatedJunctionId(),
            TemporaryId = change.TemporaryId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
