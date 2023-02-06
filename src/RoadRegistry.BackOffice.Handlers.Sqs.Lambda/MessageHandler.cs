namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Hosts;
using Requests;
using RoadSegments;

public sealed class MessageHandler : RoadRegistryMessageHandler
{
    public MessageHandler(ILifetimeScope container) : base(container)
    {
    }

    protected override SqsLambdaRequest ConvertToLambdaRequest(SqsRequest sqsRequest, string groupId) => sqsRequest switch
    {
        LinkStreetNameSqsRequest request => new LinkStreetNameSqsLambdaRequest(groupId, request),
        UnlinkStreetNameSqsRequest request => new UnlinkStreetNameSqsLambdaRequest(groupId, request),
        CorrectRoadSegmentVersionsSqsRequest request => new CorrectRoadSegmentVersionsSqsLambdaRequest(groupId, request),
        CreateRoadSegmentOutlineSqsRequest request => new CreateRoadSegmentOutlineSqsLambdaRequest(groupId, request),
        _ => throw new NotImplementedException(
            $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
    };
}
