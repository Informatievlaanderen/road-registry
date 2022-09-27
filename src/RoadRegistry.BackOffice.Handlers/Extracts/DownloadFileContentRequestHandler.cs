namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NodaTime;

public class DownloadFileContentRequestHandler : EndpointRequestHandler<DownloadFileContentRequest, DownloadFileContentResponse>
{
    private readonly RoadNetworkExtractDownloadsBlobClient _client;
    private readonly IClock _clock;
    private readonly EditorContext _context;

    public DownloadFileContentRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        RoadNetworkExtractDownloadsBlobClient client,
        IClock clock,
        ILogger<DownloadFileContentRequestHandler> logger) : base(dispatcher, logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _context = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
    }

    public override async Task<DownloadFileContentResponse> HandleAsync(DownloadFileContentRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null) throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");

        if (Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            var record = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken);
            if (record is not { Available: true })
            {
                var retryAfterSeconds = await CalculateRetryAfter(request);
                throw new DownloadExtractNotFoundException(retryAfterSeconds);
            }

            var blobName = new BlobName(record.ArchiveId);

            if (!await _client.BlobExistsAsync(blobName, cancellationToken))
                throw new BlobNotFoundException(blobName);

            var blob = await _client.GetBlobAsync(blobName, cancellationToken);
            var filename = request.DownloadId + ".zip";

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

    private async Task<int> CalculateRetryAfter(DownloadFileContentRequest request)
    {
        return await _context.ExtractUploads.TookAverageProcessDuration(_clock
                .GetCurrentInstant()
                .Minus(Duration.FromDays(request.RetryAfterAverageWindowInDays)),
            request.DefaultRetryAfter);
    }
}
