namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Abstractions.RoadNetworks;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Hosts;
using Requests;
using RoadNetworks;

public sealed class MessageHandler : RoadRegistryMessageHandler
{
    public MessageHandler(ILifetimeScope container) : base(container)
    {
    }

    protected override SqsLambdaRequest ConvertToLambdaRequest(SqsRequest sqsRequest, string groupId)
    {
        return sqsRequest switch
        {
            CreateRoadNetworkSnapshotSqsRequest request => new CreateRoadNetworkSnapshotSqsLambdaRequest(groupId, request),
            _ => throw new NotImplementedException(
                $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
        };
    }
}
