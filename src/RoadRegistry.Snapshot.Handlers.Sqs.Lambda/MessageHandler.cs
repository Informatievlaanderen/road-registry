namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda;

using Autofac;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Hosts;
using Requests;
using RoadNetworks;
using Sqs.Infrastructure;

public sealed class MessageHandler : RoadRegistryMessageHandler
{
    public MessageHandler(ILifetimeScope container) : base(container)
    {
    }

    protected override SqsLambdaRequest ConvertToLambdaRequest(SqsRequest sqsRequest, string groupId)
    {
        return sqsRequest switch
        {
            SnapshotLambdaHealthCheckSqsRequest request => new SnapshotLambdaHealthCheckSqsLambdaRequest(groupId, request),
            CreateRoadNetworkSnapshotSqsRequest request => new CreateRoadNetworkSnapshotSqsLambdaRequest(groupId, request),
            _ => throw new NotImplementedException(
                $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
        };
    }
}
