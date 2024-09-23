namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Framework;
using Handlers;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NodaTime;
using SqlStreamStore;

public class DownloadFileContentRequestHandler : EndpointRetryableRequestHandler<DownloadFileContentRequest, DownloadFileContentResponse>
{
    private readonly RoadNetworkExtractDownloadsBlobClient _client;

    public DownloadFileContentRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        RoadNetworkExtractDownloadsBlobClient client,
        IStreamStore streamStore,
        IClock clock,
        ILogger<DownloadFileContentRequestHandler> logger) : base(dispatcher, editorContext, streamStore, clock, logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    protected override async Task<DownloadFileContentResponse> InnerHandleAsync(DownloadFileContentRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null)
        {
            throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");
        }

        if (!DownloadId.TryParse(request.DownloadId, out var downloadId))
        {
            throw new InvalidGuidValidationException(nameof(request.DownloadId));
        }

        var record = await Context.ExtractDownloads.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);

        if (record is null || record is not { Available: true })
        {
            var retryAfter = await CalculateRetryAfterAsync(request, cancellationToken);
            throw new DownloadExtractNotFoundException(Convert.ToInt32(retryAfter.TotalSeconds));
        }

        if (string.IsNullOrEmpty(record.ArchiveId))
        {
            throw new ExtractArchiveNotCreatedException();
        }

        var blobName = new BlobName(record.ArchiveId);

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new BlobNotFoundException(blobName);
        }

        var blob = await _client.GetBlobAsync(blobName, cancellationToken);
        var filename = request.DownloadId + ".zip";

        var command = new Command(new DownloadRoadNetworkExtract
        {
            DownloadId = downloadId,
            ExternalRequestId = new ExternalExtractRequestId(record.ExternalRequestId)
        });
        await Dispatch(command, cancellationToken);

        return new DownloadFileContentResponse(
            filename,
            new MediaTypeHeaderValue("application/zip"),
            async (stream, actionContext) =>
            {
                await using var blobStream = await blob.OpenAsync(cancellationToken);
                await blobStream.CopyToAsync(stream, cancellationToken);
            });
    }
}
