namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extensions;
using Editor.Schema.Extracts;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public class UploadStatusRequestHandler : EndpointRequestHandler<UploadStatusRequest, UploadStatusResponse>
{
    private readonly IClock _clock;
    private readonly IStreamStore _streamStore;
    private readonly EditorContext _context;

    public UploadStatusRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        IClock clock,
        IStreamStore streamStore,
        ILogger<UploadStatusRequestHandler> logger) : base(dispatcher, logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _context = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
        _streamStore = streamStore ?? throw new ArgumentNullException(nameof(streamStore));
    }

    private async Task<int> CalculateRetryAfterAsync(UploadStatusRequest request, CancellationToken cancellationToken)
    {
        var projectionStateItem = await _context.ProjectionStates.SingleAsync(s => s.Name.Equals("roadregistry-editor-extractupload-projectionhost"), cancellationToken);

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

        return await _context.ExtractUploads
            .TookAverageProcessDuration(_clock
            .GetCurrentInstant()
            .Minus(Duration.FromDays(request.RetryAfterAverageWindowInDays)), request.DefaultRetryAfter);
    }

    public override async Task<UploadStatusResponse> HandleAsync(UploadStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParseExact(request.UploadId, "N", out var parsedUploadId))
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(request.UploadId),
                    $"'{nameof(request.UploadId)}' path parameter is not a global unique identifier without dashes.")
            });

        var record = await _context.ExtractUploads.FindAsync(new object[] { parsedUploadId }, cancellationToken);
        if (record is null)
        {
            var retryAfterSeconds = await CalculateRetryAfterAsync(request, cancellationToken);
            throw new UploadExtractNotFoundException(retryAfterSeconds);
        }

        return new UploadStatusResponse(
            record.Status switch
            {
                ExtractUploadStatus.Received => "Processing",
                ExtractUploadStatus.UploadAccepted => "Processing",
                ExtractUploadStatus.UploadRejected => "Rejected",
                ExtractUploadStatus.ChangesRejected => "Rejected",
                ExtractUploadStatus.ChangesAccepted => "Accepted",
                ExtractUploadStatus.NoChanges => "No Changes",
                _ => "Unknown"
            },
            record.Status switch
            {
                ExtractUploadStatus.Received => await CalculateRetryAfterAsync(request, cancellationToken),
                ExtractUploadStatus.UploadAccepted => await CalculateRetryAfterAsync(request, cancellationToken),
                _ => 0
            });
    }
}
