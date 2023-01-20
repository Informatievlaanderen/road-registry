namespace RoadRegistry.BackOffice.Handlers.FeatureCompare;

using Abstractions;
using Abstractions.FeatureCompare;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class FeatureCompareProcessOutputMessageRequestHandler : SqsMessageRequestHandler<FeatureCompareProcessOutputMessageRequest, FeatureCompareProcessOutputMessageResponse>
{
    public FeatureCompareProcessOutputMessageRequestHandler(CommandHandlerDispatcher dispatcher, ILogger<FeatureCompareProcessOutputMessageRequestHandler> logger) : base(dispatcher, logger)
    {
    }

    public override async Task<FeatureCompareProcessOutputMessageResponse> HandleAsync(FeatureCompareProcessOutputMessageRequest request, CancellationToken cancellationToken)
    {
        var message = new UploadRoadNetworkChangesArchive
        {
            ArchiveId = request.ArchiveId,
            FeatureCompareCompleted = true
        };
        var command = new Command(message);
        await Dispatcher(command, cancellationToken);

        _logger.LogInformation("Dispatched command {Command} for archive {ArchiveId}", nameof(UploadRoadNetworkChangesArchive), request.ArchiveId);

        return new FeatureCompareProcessOutputMessageResponse(new ArchiveId(message.ArchiveId));
    }
}
