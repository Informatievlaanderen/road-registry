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
using RoadRegistry.Infrastructure;
using TicketingService.Abstractions;

public sealed class DataValidationSqsLambdaRequestHandler : SqsLambdaHandler<DataValidationSqsLambdaRequest>
{
    private readonly IDataValidationApiClient _dataValidationApiClient;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IOrganizationCache _organizationCache;
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
        IOrganizationCache organizationCache,
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
        _organizationCache = organizationCache;
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

                        var metadata = await BuildDeliveryMetadata(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.DownloadId, cancellationToken);

                        queueItem.DataValidationId = await _dataValidationApiClient.RequestDataValidationAsync(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId, blobStream, metadata, cancellationToken);
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
                                Logger.LogInformation("Data validation delivery '{DataValidationId}' has been received, awaiting to be processed", queueItem.DataValidationId!);
                                break;
                            case ValidationJobStatus.Processing:
                                Logger.LogInformation("Data validation delivery '{DataValidationId}' is being processed", queueItem.DataValidationId!);

                                if (DataValidationConstants.ManualStages.Contains(pollResult.Stage))
                                {
                                    Logger.LogInformation("Data validation delivery '{DataValidationId}' automatic checks have completed, switching to slow polling", queueItem.DataValidationId!);
                                    automaticValidationSucceed = true;
                                }
                                break;
                            case ValidationJobStatus.Processed:
                                // The delivery is already fully processed (can happen during testing). Abort fast polling
                                // and let the slow polling service handle the approval/rejection outcome.
                                Logger.LogInformation("Data validation delivery '{DataValidationId}' is already processed, aborting fast polling and switching to slow polling", queueItem.DataValidationId!);
                                return new object();
                            case ValidationJobStatus.Error:
                                Logger.LogError("OPGEPAST! Data Validation is in Error voor levering '{DataValidationId}. Contacteer DataValidatie hiervoor.'", pollResult.Status);
                                return new object();
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

    /// <summary>
    /// Builds the context Datavalidatie stores with the delivery: which organisation collected the data, which
    /// download it belongs to and which municipality it covers. A download without an inwinningszone is not something
    /// that can happen along this path, so that one does stop the delivery.
    /// </summary>
    private async Task<DataValidationDeliveryMetadata> BuildDeliveryMetadata(DownloadId downloadId, CancellationToken cancellationToken)
    {
        var inwinningszone = await _extractsDbContext.Inwinningszones
            .SingleOrDefaultAsync(x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        if (inwinningszone is null)
        {
            throw new InvalidOperationException($"No Inwinningszone found for {downloadId}");
        }

        return new DataValidationDeliveryMetadata(
            await FindOrganizationKboNumber(inwinningszone.Operator, cancellationToken),
            downloadId.ToString(),
            inwinningszone.NisCode);
    }

    /// <summary>
    /// Resolves the KBO number to send along with the delivery. The operator on an inwinningszone is usually already a
    /// KBO number of a private surveying company, and those are not in the organization cache, so a value that is
    /// shaped like a KBO number is taken as-is and an organization the cache does not know is passed on unchanged
    /// rather than dropped.
    /// </summary>
    private async Task<string?> FindOrganizationKboNumber(string organizationCode, CancellationToken cancellationToken)
    {
        if (OrganizationKboNumber.AcceptsValue(organizationCode))
        {
            return organizationCode;
        }

        if (!OrganizationId.AcceptsValue(organizationCode))
        {
            Logger.LogWarning("Organization code '{OrganizationCode}' is not a valid organization id, sending the delivery to datavalidation without a KBO number", organizationCode);
            return null;
        }

        var organization = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(new OrganizationId(organizationCode), cancellationToken);
        if (organization?.KboNumber is null)
        {
            Logger.LogWarning("No KBO number found for organization '{OrganizationCode}', sending the delivery to datavalidation with the operator value as-is", organizationCode);
            return organizationCode;
        }

        return organization.KboNumber.Value.ToString();
    }
}
