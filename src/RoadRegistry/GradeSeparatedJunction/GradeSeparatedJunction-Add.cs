namespace RoadRegistry.GradeSeparatedJunction;

using BackOffice.Core;
using Changes;
using Events;
using RoadNetwork;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var problems = Problems.None;

        var roadNode = Create(new GradeSeparatedJunctionAdded
        {
            GradeSeparatedJunctionId = idGenerator.NewGradeSeparatedJunctionId(),
            TemporaryId = change.TemporaryId,
            LowerRoadSegmentId = idTranslator.TranslateToPermanentId(change.LowerRoadSegmentId),
            UpperRoadSegmentId = idTranslator.TranslateToPermanentId(change.UpperRoadSegmentId),
            Type = change.Type
        });

        return (roadNode, problems);
    }
}
