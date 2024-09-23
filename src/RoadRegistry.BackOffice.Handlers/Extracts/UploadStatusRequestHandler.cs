namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Microsoft.Extensions.Logging;
using NodaTime;
using Handlers;
using SqlStreamStore;

public class UploadStatusRequestHandler : EndpointRetryableRequestHandler<UploadStatusRequest, UploadStatusResponse>
{
    public UploadStatusRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        IClock clock,
        IStreamStore streamStore,
        ILogger<UploadStatusRequestHandler> logger) : base(dispatcher, editorContext, streamStore, clock, logger)
    {
    }

    protected override async Task<UploadStatusResponse> InnerHandleAsync(UploadStatusRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParseExact(request.UploadId, "N", out var parsedUploadId))
        {
            throw new InvalidGuidValidationException(nameof(request.UploadId));
        }

        var record = await Context.ExtractUploads.FindAsync(new object[] { parsedUploadId }, cancellationToken);
        if (record is null)
        {
            var retryAfter = await CalculateRetryAfterAsync(request, cancellationToken);
            throw new UploadExtractNotFoundException(Convert.ToInt32(retryAfter.TotalSeconds));
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
            Convert.ToInt32(record.Status switch
            {
                ExtractUploadStatus.Received => (await CalculateRetryAfterAsync(request, cancellationToken)).TotalSeconds,
                ExtractUploadStatus.UploadAccepted => (await CalculateRetryAfterAsync(request, cancellationToken)).TotalSeconds,
                _ => 0
            }));
    }
}
