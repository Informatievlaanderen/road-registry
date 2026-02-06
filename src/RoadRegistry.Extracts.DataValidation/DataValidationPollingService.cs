namespace RoadRegistry.Extracts.DataValidation;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure;
using Schema;

public class DataValidationPollingService : IScheduledJob
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IDataValidationApiClient _dataValidationApiClient;
    private readonly ILogger _logger;

    public DataValidationPollingService(
        ExtractsDbContext extractsDbContext,
        IDataValidationApiClient dataValidationApiClient,
        ILoggerFactory loggerFactory)
    {
        _extractsDbContext = extractsDbContext;
        _dataValidationApiClient = dataValidationApiClient;
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
                //TODO-pr poll datavalidation, update extract status if needed


                //TODO-pr temp
                await _extractsDbContext.ManualValidationFailedAsync(new UploadId(queueItem.UploadId), cancellationToken);

                // await _extractsDbContext.UploadAcceptedAsync(new UploadId(queueItem.UploadId), cancellationToken);
                //
                // queueItem.Completed = true;
                // await _extractsDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while polling data validation [UploadId={queueItem.UploadId}, DataValidationId={queueItem.DataValidationId}]: {ex.Message}");
            }
        }
    }
}
