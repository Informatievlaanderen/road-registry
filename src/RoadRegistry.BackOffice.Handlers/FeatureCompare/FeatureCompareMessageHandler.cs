namespace RoadRegistry.BackOffice.Handlers.FeatureCompare
{
    using Microsoft.Extensions.Logging;
    using RoadRegistry.BackOffice.Abstractions;
    using RoadRegistry.BackOffice.Framework;
    using RoadRegistry.BackOffice.Messages;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.FeatureCompare;

    public class FeatureCompareMessageHandler : SqsMessageRequestHandler<FeatureCompareMessageRequest, FeatureCompareMessageResponse>
    {
        public FeatureCompareMessageHandler(CommandHandlerDispatcher dispatcher, ILogger<FeatureCompareMessageHandler> logger) : base(dispatcher, logger) {
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
