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
            _logger.LogInformation("Polling data validation ID {Id}", queueItem.DataValidationId);
            try
            {
                bool? uploadAccepted = null;
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
                            switch (pollResult.Result)
                            {
                                case ValidationResult.Approved:
                                case ValidationResult.ApprovedWithRemarks:
                                    uploadAccepted = true;
                                    break;
                                case ValidationResult.Rejected:
                                case ValidationResult.AutomaticallyRejected:
                                    //qualityReportUrl = $"https://geckoqualityreportstest.blob.core.windows.net/kwaliteitsrapporten/{queueItem.DataValidationId!}.html";
                                    var artifactsResponse = await _dataValidationApiClient.GetDeliveryArtifactsAsync(queueItem.DataValidationId!, cancellationToken);
                                    qualityReportUrl = artifactsResponse.Artifacts.Single(x => x.Type == DeliveryArtifactType.QualityReport).Url;

                                    ticketError = new TicketError("De oplading is mislukt. Gelieve het kwaliteitsrapport te openen voor meer informatie.", "DataValidationRejected");
                                    uploadAccepted = false;
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
                        await _mediator.Send(sqsRequest, cancellationToken);
                    }
                    else
                    {
                        await _extractsDbContext.ManualValidationFailedAsync(new UploadId(queueItem.UploadId), qualityReportUrl!, cancellationToken);
                        await _ticketing.Error(sqsRequest.TicketId, ticketError!, cancellationToken);
                    }

                    queueItem.Completed = true;
                    await _extractsDbContext.SaveChangesAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while polling data validation [UploadId={queueItem.UploadId}, DataValidationId={queueItem.DataValidationId}]: {ex.Message}");
            }
        }
    }
}
