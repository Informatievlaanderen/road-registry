namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NodaTime;
using Handlers;
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

    public override async Task<DownloadFileContentResponse> HandleAsync(DownloadFileContentRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null) throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");

        if (Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            var record = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken);

            if (record is null || record is not { Available: true })
            {
                var retryAfterSeconds = await CalculateRetryAfterAsync(request, cancellationToken);
                throw new DownloadExtractNotFoundException(retryAfterSeconds);
            }

            var blobName = new BlobName(record.ArchiveId);

            if (!await _client.BlobExistsAsync(blobName, cancellationToken))
                throw new BlobNotFoundException(blobName);

            var blob = await _client.GetBlobAsync(blobName, cancellationToken);
            var filename = request.DownloadId + ".zip";

            var command = new Command(new DownloadRoadNetworkExtract
            {
                DownloadId = new DownloadId(parsedDownloadId),
                ExternalRequestId = record.ExternalRequestId
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

        throw new ValidationException(new[]
        {
            new ValidationFailure(
                nameof(request.DownloadId),
                $"'{nameof(request.DownloadId)}' path parameter is not a global unique identifier without dashes.")
        });
    }
}
