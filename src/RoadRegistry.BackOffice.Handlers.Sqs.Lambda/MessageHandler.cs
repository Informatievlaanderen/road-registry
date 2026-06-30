namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Autofac;
using Be.Vlaanderen.Basisregisters.Aws.Lambda;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using MediatR;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CloseExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CreateRoadSegmentOutlineV2;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.DataValidation;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.LinkRoadSegmentsToStreetNameIds;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateDryRunRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestInwinningExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadInwinningExtract;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;
using RoadRegistry.BackOffice.Uploads;

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
            messageMetadata.Logger?.LogError($"Unable to cast '{nameof(messageData)}' as {nameof(SqsRequest)}.");
            return;
        }

        await using var lifetimeScope = _container.BeginLifetimeScope();
        var mediator = lifetimeScope.Resolve<IMediator>();

        var sqsLambdaRequest = ConvertToLambdaRequest(sqsRequest, messageMetadata.MessageGroupId!);
        await mediator.Send(sqsLambdaRequest, cancellationToken);
    }

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
            UploadExtractSqsRequestV2 request => new UploadExtractSqsLambdaRequestV2(groupId, request),
            UploadInwinningExtractSqsRequest request => new UploadInwinningExtractSqsLambdaRequest(groupId, request),
            CloseExtractSqsRequest request => new CloseExtractSqsLambdaRequest(groupId, request),
            ChangeRoadNetworkSqsRequest request => new ChangeRoadNetworkSqsLambdaRequest(groupId, request),
            RemoveRoadSegmentsSqsRequest request => new RemoveRoadSegmentsSqsLambdaRequest(groupId, request),
            SystemLinkRoadSegmentsToStreetNameIdsSqsRequest request => new SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequest(groupId, request),
            MigrateDryRunRoadNetworkSqsRequest request => new MigrateDryRunRoadNetworkSqsLambdaRequest(groupId, request),
            MigrateRoadNetworkSqsRequest request => new MigrateRoadNetworkSqsLambdaRequest(groupId, request),
            DataValidationSqsRequest request => new DataValidationSqsLambdaRequest(groupId, request),
            CreateRoadSegmentOutlineV2SqsRequest request => new CreateRoadSegmentOutlineV2SqsLambdaRequest(groupId, request),
            _ => throw new NotImplementedException(
                $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
        };
    }
}
