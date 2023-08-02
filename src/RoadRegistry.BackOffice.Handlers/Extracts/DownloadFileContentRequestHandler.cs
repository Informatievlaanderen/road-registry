namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Editor.Schema.Extensions;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NodaTime;
using SqlStreamStore;

public class DownloadFileContentRequestHandler : EndpointRequestHandler<DownloadFileContentRequest, DownloadFileContentResponse>
{
    private readonly RoadNetworkExtractDownloadsBlobClient _client;
    private readonly IClock _clock;
    private readonly IStreamStore _streamStore;
    private readonly EditorContext _context;

    public DownloadFileContentRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        RoadNetworkExtractDownloadsBlobClient client,
        IStreamStore streamStore,
        IClock clock,
        ILogger<DownloadFileContentRequestHandler> logger) : base(dispatcher, logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _context = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
        _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
    }

    private async Task<int> CalculateRetryAfterAsync(DownloadFileContentRequest request, CancellationToken cancellationToken)
    {
        var projectionStateItem = await _context.ProjectionStates.SingleOrDefaultAsync(s => s.Name.Equals("roadregistry-editor-extractdownload-projectionhost"), cancellationToken);

        if (projectionStateItem is not null)
        {
            var currentPosition = projectionStateItem.Position;
            var lastPosition = await _streamStore.ReadHeadPosition(cancellationToken);

            if (currentPosition < lastPosition)
            {
                var eventProcessorMetrics = await _context.EventProcessorMetrics.GetMetricsAsync("ExtractUploadEventProcessor", cancellationToken);
                if (eventProcessorMetrics is not null)
                {
                    var averageTimePerEvent = eventProcessorMetrics.ElapsedMilliseconds / eventProcessorMetrics.ToPosition;
                    var estimatedTimeRemaining = averageTimePerEvent * (lastPosition - currentPosition);
                    return Convert.ToInt32(estimatedTimeRemaining) + (3 * 60 * 1000); // Added 3 minute buffer
                }
            }
        }

        return await _context.ExtractUploads
            .TookAverageProcessDuration(_clock
            .GetCurrentInstant()
            .Minus(Duration.FromDays(request.RetryAfterAverageWindowInDays)), request.DefaultRetryAfter);
    }

    public override async Task<DownloadFileContentResponse> HandleAsync(DownloadFileContentRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null) throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");

        if (Guid.TryParseExact(request.DownloadId, "N", out var parsedDownloadId))
        {
            var record = await _context.ExtractDownloads.FindAsync(new object[] { parsedDownloadId }, cancellationToken);
            if (record is not { Available: true })
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
            await Dispatcher(command, cancellationToken);

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
