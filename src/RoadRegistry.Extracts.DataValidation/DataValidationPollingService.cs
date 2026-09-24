namespace RoadRegistry.Extracts.DataValidation;

using BackOffice.Handlers.Sqs.RoadNetwork;
using BackOffice.Uploads;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Infrastructure;
using Schema;
using TicketingService.Abstractions;

public class DataValidationPollingService : IScheduledJob
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IDataValidationApiClient _dataValidationApiClient;
    private readonly IMediator _mediator;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly ITicketing _ticketing;
    private readonly UseDataValidationFeatureToggle _useDataValidationFeatureToggle;
    private readonly ILogger _logger;

    public DataValidationPollingService(
        ExtractsDbContext extractsDbContext,
        IDataValidationApiClient dataValidationApiClient,
        IMediator mediator,
        SqsJsonMessageSerializer sqsJsonMessageSerializer,
        ITicketing ticketing,
        UseDataValidationFeatureToggle useDataValidationFeatureToggle,
        ILoggerFactory loggerFactory)
    {
        _extractsDbContext = extractsDbContext;
        _dataValidationApiClient = dataValidationApiClient;
        _mediator = mediator;
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _ticketing = ticketing;
        _useDataValidationFeatureToggle = useDataValidationFeatureToggle;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var queueItems = await _extractsDbContext.DataValidationQueue
            .Where(x => !x.Completed && x.DataValidationId != null)
            .ToListAsync(cancellationToken);

        foreach (var queueItem in queueItems)
        {
            try
            {
                // A rejected delivery is kept in the queue because Datavalidatie can reopen it, but not once the
                // uploader has moved on: a new upload for the same extract is a delivery of its own.
                if (queueItem.RejectedOn is not null && await UploaderStartedANewUpload(queueItem, cancellationToken))
                {
                    _logger.LogInformation("Stop polling data validation ID {Id}, a new upload was started for this extract", queueItem.DataValidationId);

                    queueItem.Completed = true;
                    await _extractsDbContext.SaveChangesAsync(cancellationToken);
                    continue;
                }

                _logger.LogInformation("Polling data validation ID {Id}", queueItem.DataValidationId);

                bool? uploadAccepted = null;
                var rejectionIsFinal = false;
                TicketError? ticketError = null;
                string? qualityReportUrl = null;

                if (_useDataValidationFeatureToggle.FeatureEnabled)
                {
                    var pollResult = await _dataValidationApiClient.PollDeliveryAsync(queueItem.DataValidationId!, cancellationToken);
                    switch (pollResult.Status)
                    {
                        case ValidationJobStatus.Received:
                        case ValidationJobStatus.Processing:
                            // keep on waiting
                            break;
                        case ValidationJobStatus.Processed:
                            // Always keep the quality report, both on approval and rejection, so the UI shows at least one.
                            var artifactsResponse = await _dataValidationApiClient.GetDeliveryArtifactsAsync(queueItem.DataValidationId!, cancellationToken);
                            qualityReportUrl = artifactsResponse.Artifacts.Single(x => x.Type == DeliveryArtifactType.QualityReport).Url;

                            switch (pollResult.Result)
                            {
                                case ValidationResult.Approved:
                                case ValidationResult.ApprovedWithRemarks:
                                    uploadAccepted = true;
                                    break;
                                case ValidationResult.Rejected:
                                    // Datavalidatie can reopen a delivery it rejected by mistake and approve it after all,
                                    // keeping the same deliveryId. The rejection is reported once and we keep polling.
                                    ticketError = new TicketError("De oplading is mislukt. Gelieve het kwaliteitsrapport te openen voor meer informatie.", "DataValidationRejected");
                                    uploadAccepted = false;
                                    break;
                                case ValidationResult.AutomaticallyRejected:
                                    // An automatic rejection is not reopened, so there is nothing left to wait for.
                                    ticketError = new TicketError("De oplading is mislukt. Gelieve het kwaliteitsrapport te openen voor meer informatie.", "DataValidationRejected");
                                    uploadAccepted = false;
                                    rejectionIsFinal = true;
                                    break;
                            }
                            break;

                        case ValidationJobStatus.Error:
                            _logger.LogError("OPGEPAST! Data Validation is in Error voor levering '{DataValidationId}. Contacteer DataValidatie hiervoor.'", pollResult.Status);
                            break;
                        default:
                            _logger.LogError("Unknown data validation status '{Status}' for delivery '{DataValidationId}'", pollResult.Status, queueItem.DataValidationId!);
                            break;
                    }
                }
                else
                {
                    uploadAccepted = true;
                }

                if (uploadAccepted is not null)
                {
                    var sqsRequest = (MigrateRoadNetworkSqsRequest)_sqsJsonMessageSerializer.Deserialize(queueItem.SqsRequestJson)!;

                    if (uploadAccepted.Value)
                    {
                        if (qualityReportUrl is not null)
                        {
                            await _extractsDbContext.SetQualityReportUrlAsync(new UploadId(queueItem.UploadId), qualityReportUrl, cancellationToken);
                        }

                        if (queueItem.RejectedOn is not null)
                        {
                            // The delivery was rejected before and has been reopened and approved after all. Put the upload
                            // and its ticket back where an approval expects to find them: the portal reads the result from
                            // the ticket, and a later refusal by the road network has to read as a processing failure
                            // instead of a validation failure (see ExtractUploadStatusTransitions.OnRoadNetworkChangesRejected).
                            _logger.LogInformation("Data validation delivery '{DataValidationId}' was approved after it was rejected on {RejectedOn}", queueItem.DataValidationId!, queueItem.RejectedOn);

                            await _extractsDbContext.AutomaticValidationSucceededAsync(new UploadId(queueItem.UploadId), cancellationToken);
                            await _ticketing.Pending(sqsRequest.TicketId, new TicketResult(new
                            {
                                Status = nameof(ExtractUploadStatus.AutomaticValidationSucceeded)
                            }), cancellationToken);
                        }

                        await _mediator.Send(sqsRequest, cancellationToken);

                        queueItem.Completed = true;
                        await _extractsDbContext.SaveChangesAsync(cancellationToken);
                    }
                    else if (queueItem.RejectedOn is null)
                    {
                        await _extractsDbContext.ManualValidationFailedAsync(new UploadId(queueItem.UploadId), qualityReportUrl!, cancellationToken);
                        await _ticketing.Error(sqsRequest.TicketId, ticketError!, cancellationToken);

                        queueItem.RejectedOn = DateTimeOffset.UtcNow;
                        // Only an automatic rejection closes the queue item; a rejection by Datavalidatie is polled on.
                        queueItem.Completed = rejectionIsFinal;
                        await _extractsDbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while polling data validation [UploadId={queueItem.UploadId}, DataValidationId={queueItem.DataValidationId}]: {ex.Message}");
            }
        }
    }

    // Asking for a new upload URL already clears LatestUploadId, so from the moment the uploader starts a new upload
    // through the portal the delivery behind the previous upload no longer interests anyone. An upload or a download we
    // cannot find says nothing, and does not stop the polling either.
    private Task<bool> UploaderStartedANewUpload(DataValidationQueueItem queueItem, CancellationToken cancellationToken)
    {
        return _extractsDbContext.ExtractUploads
            .AsNoTracking()
            .Where(upload => upload.UploadId == queueItem.UploadId)
            .Join(_extractsDbContext.ExtractDownloads,
                upload => upload.DownloadId,
                download => download.DownloadId,
                (_, download) => download.LatestUploadId)
            .AnyAsync(latestUploadId => latestUploadId != queueItem.UploadId, cancellationToken);
    }
}
