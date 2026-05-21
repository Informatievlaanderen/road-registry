namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.DataValidation;

using System.Diagnostics;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using TicketingService.Abstractions;

public sealed class DataValidationSqsLambdaRequestHandler : SqsLambdaHandler<DataValidationSqsLambdaRequest>
{
    private readonly IDataValidationApiClient _dataValidationApiClient;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly RoadNetworkUploadsBlobClient _uploadsBlobClient;
    private readonly UseDataValidationFeatureToggle _useDataValidationFeatureToggle;

    public DataValidationSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDataValidationApiClient dataValidationApiClient,
        ExtractsDbContext extractsDbContext,
        SqsJsonMessageSerializer sqsJsonMessageSerializer,
        RoadNetworkUploadsBlobClient uploadsBlobClient,
        UseDataValidationFeatureToggle useDataValidationFeatureToggle,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Error)
    {
        _dataValidationApiClient = dataValidationApiClient;
        _extractsDbContext = extractsDbContext;
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _uploadsBlobClient = uploadsBlobClient;
        _useDataValidationFeatureToggle = useDataValidationFeatureToggle;
    }

    protected override async Task<object> InnerHandle(DataValidationSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        try
        {
            var startOfAction = Stopwatch.StartNew();

            var queueItem = await _extractsDbContext.DataValidationQueue.SingleOrDefaultAsync(x => x.UploadId == sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId.ToGuid(), cancellationToken);
            if (queueItem is null)
            {
                queueItem = new DataValidationQueueItem
                {
                    UploadId = sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId,
                    SqsRequestJson = _sqsJsonMessageSerializer.Serialize(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest)
                };
                _extractsDbContext.DataValidationQueue.Add(queueItem);
                await _extractsDbContext.SaveChangesAsync(cancellationToken);
            }

            if (queueItem.DataValidationId is null)
            {
                try
                {
                    if (_useDataValidationFeatureToggle.FeatureEnabled)
                    {
                        var blob = await _uploadsBlobClient.GetBlobAsync(new BlobName(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId.ToString()), cancellationToken);
                        await using var blobStream = await blob.OpenAsync(cancellationToken);

                        queueItem.DataValidationId = await _dataValidationApiClient.RequestDataValidationAsync(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId, blobStream, cancellationToken);
                    }
                    else
                    {
                        queueItem.DataValidationId = $"DUMMY_{Guid.NewGuid()}";
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, $"Error while requesting datavalidation for download id {sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.DownloadId}");
                    throw;
                }

                await _extractsDbContext.SaveChangesAsync(cancellationToken);
            }

            bool? automaticValidationSucceed = null;
            TicketError? ticketError = null;
            string? qualityReportUrl = null;

            while (automaticValidationSucceed is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                if (startOfAction.Elapsed.TotalMinutes >= 10)
                {
                    Logger.LogInformation("Stop waiting, switching to slow polling");
                    break;
                }

                if (_useDataValidationFeatureToggle.FeatureEnabled)
                {
                    try
                    {
                        var pollResult = await _dataValidationApiClient.PollDeliveryAsync(queueItem.DataValidationId!, cancellationToken);
                        switch (pollResult.Status)
                        {
                            case ValidationJobStatus.Received:
                                Logger.LogInformation("Data validation delivery '{DataValidationId} has been received, awaiting to be processed", queueItem.DataValidationId!);
                                break;
                            case ValidationJobStatus.Processing:
                                Logger.LogInformation("Data validation delivery '{DataValidationId} is being processed", queueItem.DataValidationId!);

                                if (DataValidationConstants.ManualStages.Contains(pollResult.Stage))
                                {
                                    Logger.LogInformation("Data validation delivery '{DataValidationId} automatic checks have completed, switching to slow polling", queueItem.DataValidationId!);
                                    automaticValidationSucceed = true;
                                }
                                break;
                            case ValidationJobStatus.Error:
                                Logger.LogError("UNEXPECTED data validation status '{Status}' for delivery '{DataValidationId}'", pollResult.Status, queueItem.DataValidationId!);
                                ticketError = new TicketError("Een onbekend probleem heeft zich voorgedaan bij de validatie. Wij zijn hiervan op de hoogte gebracht.", "DataValidationError");
                                automaticValidationSucceed = false;
                                break;
                            default:
                                Logger.LogError("Unknown data validation status '{Status}' for delivery '{DataValidationId}'", pollResult.Status, queueItem.DataValidationId!);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, $"Error while polling data validation [UploadId={queueItem.UploadId}, DownloadId={sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.DownloadId}, DataValidationId={queueItem.DataValidationId}]: {ex.Message}");
                        throw;
                    }
                }
                else
                {
                    automaticValidationSucceed = true;
                }
            }

            if (automaticValidationSucceed is not null)
            {
                if (automaticValidationSucceed.Value)
                {
                    await _extractsDbContext.AutomaticValidationSucceededAsync(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId, cancellationToken);
                    await Ticketing.Pending(sqsLambdaRequest.TicketId, new TicketResult(new
                    {
                        Status = nameof(ExtractUploadStatus.AutomaticValidationSucceeded)
                    }), cancellationToken);
                }
                else
                {
                    await _extractsDbContext.AutomaticValidationFailedAsync(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId, qualityReportUrl, cancellationToken);
                    await Ticketing.Error(sqsLambdaRequest.TicketId, ticketError!, cancellationToken);

                    queueItem.Completed = true;
                    await _extractsDbContext.SaveChangesAsync(cancellationToken);
                }
            }

            return new object();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while checking with datavalidation with download id {sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.DownloadId}");
            throw;
        }
    }
}
