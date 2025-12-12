namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Extracts;
using MediatR;
using Requests;
using Requests.Extracts;
using Requests.RoadNetwork;
using RoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Infrastructure;
using RoadSegments;

public sealed class MessageHandler : BlobMessageHandler
{
    private readonly ILifetimeScope _container;

    public MessageHandler(ILifetimeScope container, SqsMessagesBlobClient blobClient) : base(blobClient)
    {
        _container = container;
    }

    protected override async Task HandleMessageAsync(object? messageData, MessageMetadata messageMetadata, CancellationToken cancellationToken)
    {
        if (messageData is not SqsRequest sqsRequest)
        {
            messageMetadata.Logger?.LogInformation($"Unable to cast '{nameof(messageData)}' as {nameof(SqsRequest)}.");
            return;
        }

        await using var lifetimeScope = _container.BeginLifetimeScope();
        var mediator = lifetimeScope.Resolve<IMediator>();

        var sqsLambdaRequest = ConvertToLambdaRequest(sqsRequest, messageMetadata.MessageGroupId!);
        await mediator.Send(sqsLambdaRequest, cancellationToken);
    }

    //TODO-pr separate out to add unit test to ensure all SqsRequests in assembly can be converted
    private static SqsLambdaRequest ConvertToLambdaRequest(SqsRequest sqsRequest, string groupId)
    {
        return sqsRequest switch
        {
            BackOfficeLambdaHealthCheckSqsRequest request => new BackOfficeLambdaHealthCheckSqsLambdaRequest(groupId, request),
            LinkStreetNameSqsRequest request => new LinkStreetNameSqsLambdaRequest(groupId, request),
            UnlinkStreetNameSqsRequest request => new UnlinkStreetNameSqsLambdaRequest(groupId, request),
            CreateRoadSegmentOutlineSqsRequest request => new CreateRoadSegmentOutlineSqsLambdaRequest(groupId, request),
            DeleteRoadSegmentOutlineSqsRequest request => new DeleteRoadSegmentOutlineSqsLambdaRequest(groupId, request),
            ChangeRoadSegmentAttributesSqsRequest request => new ChangeRoadSegmentAttributesSqsLambdaRequest(groupId, request),
            ChangeRoadSegmentOutlineGeometrySqsRequest request => new ChangeRoadSegmentOutlineGeometrySqsLambdaRequest(groupId, request),
            ChangeRoadSegmentsDynamicAttributesSqsRequest request => new ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest(groupId, request),
            RequestExtractSqsRequest request => new RequestExtractSqsLambdaRequest(groupId, request),
            RequestInwinningExtractSqsRequest request => new RequestInwinningExtractSqsLambdaRequest(groupId, request),
            UploadExtractSqsRequest request => new UploadExtractSqsLambdaRequest(groupId, request),
            CloseExtractSqsRequest request => new CloseExtractSqsLambdaRequest(groupId, request),
            ChangeRoadNetworkSqsRequest request => new ChangeRoadNetworkCommandSqsLambdaRequest(groupId, request),
            RemoveRoadSegmentsSqsRequest request => new RemoveRoadSegmentsCommandSqsLambdaRequest(groupId, request),
            _ => throw new NotImplementedException(
                $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
        };
    }
}
