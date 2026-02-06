namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.DataValidation;

using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using TicketingService.Abstractions;

public sealed class DataValidationSqsLambdaRequestHandler : SqsLambdaHandler<DataValidationSqsLambdaRequest>
{
    private readonly IDataValidationApiClient _dataValidationClient;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;

    public DataValidationSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDataValidationApiClient dataValidationClient,
        ExtractsDbContext extractsDbContext,
        SqsJsonMessageSerializer sqsJsonMessageSerializer,
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
        _dataValidationClient = dataValidationClient;
        _extractsDbContext = extractsDbContext;
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
    }

    protected override async Task<object> InnerHandle(DataValidationSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
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
            queueItem.DataValidationId = await _dataValidationClient.RequestDataValidationAsync(cancellationToken);
            await _extractsDbContext.SaveChangesAsync(cancellationToken);
        }

        //TODO-pr poll until automatic validation is complete/rejected, with max 10mins
        //if automaticvalidation failed, then reject extractrequest
        //await _extractRequests.AutomaticValidationFailedAsync(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest.UploadId, cancellationToken);
        //queueItem.Completed = true;
        //await _extractsDbContext.SaveChangesAsync(cancellationToken);

        //if automaticvalidation passed, then keep going with slower polling by simply stopping this lambda run

        return new object();
    }
}
