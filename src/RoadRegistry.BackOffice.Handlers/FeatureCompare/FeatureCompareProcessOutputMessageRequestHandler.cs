namespace RoadRegistry.BackOffice.Handlers.FeatureCompare;

using Abstractions;
using Abstractions.FeatureCompare;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public class FeatureCompareProcessOutputMessageRequestHandler : SqsMessageRequestHandler<FeatureCompareProcessOutputMessageRequest, FeatureCompareProcessOutputMessageResponse>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;

    public FeatureCompareProcessOutputMessageRequestHandler(IRoadNetworkCommandQueue commandQueue, ILogger<FeatureCompareProcessOutputMessageRequestHandler> logger)
        : base(logger)
    {
        _commandQueue = commandQueue;
    }

    public override async Task<FeatureCompareProcessOutputMessageResponse> HandleAsync(FeatureCompareProcessOutputMessageRequest request, CancellationToken cancellationToken)
    {
        var message = new UploadRoadNetworkChangesArchive
        {
            ArchiveId = request.ArchiveId,
            FeatureCompareCompleted = true
        };

        var command = new Command(message);
        await _commandQueue.Write(command, cancellationToken);
        
        Logger.LogInformation("Command queued {Command} for archive {ArchiveId}", nameof(UploadRoadNetworkChangesArchive), request.ArchiveId);

        return new FeatureCompareProcessOutputMessageResponse(new ArchiveId(message.ArchiveId));
    }
}
