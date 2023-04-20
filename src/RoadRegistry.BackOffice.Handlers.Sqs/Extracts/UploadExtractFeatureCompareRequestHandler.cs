namespace RoadRegistry.BackOffice.Handlers.Sqs.Extracts;

using Abstractions.Configuration;
using Abstractions.Exceptions;
using BackOffice.Uploads;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Uploads;
using Uploads;
using UploadExtractFeatureCompareRequest = Abstractions.Extracts.UploadExtractFeatureCompareRequest;

/// <summary>
///     Post upload extract controller
/// </summary>
public class UploadExtractFeatureCompareRequestHandler : UploadExtractFeatureCompareRequestHandlerBase<UploadExtractFeatureCompareRequest>
{
    private readonly EditorContext _context;

    public UploadExtractFeatureCompareRequestHandler(
        FeatureCompareMessagingOptions messagingOptions,
        CommandHandlerDispatcher dispatcher,
        RoadNetworkFeatureCompareBlobClient client,
        ISqsQueuePublisher sqsQueuePublisher,
        IZipArchiveBeforeFeatureCompareValidator validator,
        IRoadNetworkEventWriter roadNetworkEventWriter,
        EditorContext context,
        ILogger<UploadExtractFeatureCompareRequestHandler> logger)
        : base(messagingOptions, dispatcher, client, sqsQueuePublisher, validator, roadNetworkEventWriter, logger)
    {
        _context = context.ThrowIfNull();
    }

    public override async Task<UploadExtractFeatureCompareResponse> HandleAsync(UploadExtractFeatureCompareRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null)
        {
            throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");
        }

        return await base.HandleAsync(request, cancellationToken);
    }

    protected override async Task<object> BuildSqsMessage(UploadExtractFeatureCompareRequest request, ArchiveId archiveId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            throw new UploadExtractNotFoundException($"Could not upload the extract with filename {request.Archive.FileName}");
        }

        var download = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken)
                       ?? throw new ExtractDownloadNotFoundException(new DownloadId(parsedDownloadId));

        return new UploadRoadNetworkExtractChangesArchive
        {
            RequestId = download.RequestId,
            DownloadId = download.DownloadId,
            UploadId = archiveId.ToGuid(),
            ArchiveId = archiveId.ToString()
        };
    }
}
