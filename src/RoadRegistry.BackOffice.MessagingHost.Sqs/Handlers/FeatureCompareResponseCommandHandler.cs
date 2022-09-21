namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Handlers
{
    using RoadRegistry.BackOffice.Abstractions;
    using RoadRegistry.BackOffice.Framework;
    using RoadRegistry.BackOffice.Messages;
    using RoadRegistry.BackOffice.MessagingHost.Sqs.Requests;
    using RoadRegistry.BackOffice.MessagingHost.Sqs.Responses;
    using System.Threading;

    public class FeatureCompareMessageRequestHandler : SqsMessageRequestHandler<FeatureCompareMessageRequest, FeatureCompareMessageResponse>
    {
        public FeatureCompareMessageRequestHandler(CommandHandlerDispatcher dispatcher, ILogger<FeatureCompareMessageRequestHandler> logger) : base(dispatcher, logger) {
        }

        public override async Task<FeatureCompareMessageResponse> HandleAsync(FeatureCompareMessageRequest request, CancellationToken cancellationToken)
        {
            var message = new UploadRoadNetworkChangesArchive
            {
                ArchiveId = request.ArchiveId
            };
            var command = new Command(message);
            await Dispatcher(command, cancellationToken);

            return new FeatureCompareMessageResponse(new ArchiveId(message.ArchiveId));
        }
    }
}
