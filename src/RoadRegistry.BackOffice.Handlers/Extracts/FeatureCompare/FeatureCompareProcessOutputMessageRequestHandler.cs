namespace RoadRegistry.BackOffice.Handlers.Extracts.FeatureCompare;

using Abstractions;
using Abstractions.Extracts.FeatureCompare;
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
        var message = new UploadRoadNetworkExtractChangesArchive
        {
            ArchiveId = request.ArchiveId,
            RequestId = request.RequestId,
            DownloadId = request.DownloadId,
            UploadId = request.UploadId,
            FeatureCompareCompleted = true
        };

        var command = new Command(message);
        await _commandQueue.WriteAsync(command, cancellationToken);
        
        Logger.LogInformation("Command queued {Command} for archive {ArchiveId}", message.GetType().Name, request.ArchiveId);

        return new FeatureCompareProcessOutputMessageResponse(new ArchiveId(message.ArchiveId));
    }
}
