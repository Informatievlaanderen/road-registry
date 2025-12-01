namespace RoadRegistry.GradeSeparatedJunction;

using Changes;
using Events;
using RoadNetwork;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var problems = Problems.None;

        var roadNode = Create(new GradeSeparatedJunctionAdded
        {
            GradeSeparatedJunctionId = idGenerator.NewGradeSeparatedJunctionId(),
            OriginalId = change.TemporaryId,
            LowerRoadSegmentId = idTranslator.TranslateToPermanentId(change.LowerRoadSegmentId),
            UpperRoadSegmentId = idTranslator.TranslateToPermanentId(change.UpperRoadSegmentId),
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
