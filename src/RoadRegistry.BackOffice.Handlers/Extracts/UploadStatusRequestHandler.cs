namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Extensions;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Microsoft.Extensions.Logging;
using NodaTime;

public class UploadStatusRequestHandler : EndpointRequestHandler<UploadStatusRequest, UploadStatusResponse>
{
    private readonly IClock _clock;
    private readonly EditorContext _context;

    public UploadStatusRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        IClock clock,
        ILogger<UploadStatusRequestHandler> logger) : base(dispatcher, logger)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _context = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
    }

    private async Task<int> CalculateRetryAfter(UploadStatusRequest request)
    {
        return await _context.ExtractUploads.TookAverageProcessDuration(_clock
                .GetCurrentInstant()
                .Minus(Duration.FromDays(request.RetryAfterAverageWindowInDays)),
            request.DefaultRetryAfter);
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
            var retryAfterSeconds = await CalculateRetryAfter(request);
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
                ExtractUploadStatus.Received => await CalculateRetryAfter(request),
                ExtractUploadStatus.UploadAccepted => await CalculateRetryAfter(request),
                _ => 0
            });
    }
}
