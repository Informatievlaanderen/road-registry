namespace RoadRegistry.Extracts.DataValidation;

using BackOffice.Handlers.Sqs.RoadNetwork;
using BackOffice.Uploads;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger _logger;

    public DataValidationPollingService(
        ExtractsDbContext extractsDbContext,
        IDataValidationApiClient dataValidationApiClient,
        IMediator mediator,
        SqsJsonMessageSerializer sqsJsonMessageSerializer,
        ITicketing ticketing,
        ILoggerFactory loggerFactory)
    {
        _extractsDbContext = extractsDbContext;
        _dataValidationApiClient = dataValidationApiClient;
        _mediator = mediator;
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _ticketing = ticketing;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var queueItems =  await _extractsDbContext.DataValidationQueue
            .Where(x => !x.Completed && x.DataValidationId != null)
            .ToListAsync(cancellationToken);

        foreach (var queueItem in queueItems)
        {
            _logger.LogInformation("Polling data validation ID {Id}", queueItem.DataValidationId);
            try
            {
                bool? uploadAccepted = null;
                TicketError? ticketError = null;

                //TODO-pr poll datavalidation, update extract status if needed
                //temp force to true to be able to continue
                uploadAccepted = true;

                if (uploadAccepted is not null)
                {
                    var sqsRequest = (MigrateRoadNetworkSqsRequest)_sqsJsonMessageSerializer.Deserialize(queueItem.SqsRequestJson)!;

                    if (uploadAccepted.Value)
                    {
                        await _mediator.Send(sqsRequest, cancellationToken);
                    }
                    else
                    {
                        await _extractsDbContext.ManualValidationFailedAsync(new UploadId(queueItem.UploadId), cancellationToken);
                        await _ticketing.Error(sqsRequest.TicketId, ticketError, cancellationToken);
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
