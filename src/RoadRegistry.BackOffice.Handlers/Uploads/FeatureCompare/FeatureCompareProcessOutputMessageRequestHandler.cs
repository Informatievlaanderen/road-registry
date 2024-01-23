namespace RoadRegistry.BackOffice.Handlers.Uploads.FeatureCompare;

using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Uploads.FeatureCompare;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;

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
        await _commandQueue.WriteAsync(command, cancellationToken);
        
        Logger.LogInformation("Command queued {Command} for archive {ArchiveId}", message.GetType().Name, request.ArchiveId);

        return new FeatureCompareProcessOutputMessageResponse(new ArchiveId(message.ArchiveId));
    }
}
