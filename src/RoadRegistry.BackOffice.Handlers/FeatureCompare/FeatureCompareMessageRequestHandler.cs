namespace RoadRegistry.BackOffice.Handlers.FeatureCompare;

using Abstractions;
using Abstractions.FeatureCompare;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class FeatureCompareMessageRequestHandler : SqsMessageRequestHandler<FeatureCompareMessageRequest, FeatureCompareMessageResponse>
{
    public FeatureCompareMessageRequestHandler(CommandHandlerDispatcher dispatcher, ILogger<FeatureCompareMessageRequestHandler> logger) : base(dispatcher, logger)
    {
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