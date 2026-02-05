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
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using TicketingService.Abstractions;

public sealed class DataValidationSqsLambdaRequestHandler : SqsLambdaHandler<DataValidationSqsLambdaRequest>
{
    private readonly IDataValidationApiClient _dataValidationClient;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IExtractRequests _extractRequests;
    private readonly SqsJsonMessageSerializer _sqsJsonMessageSerializer;
    private readonly IMediator _mediator;

    public DataValidationSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDataValidationApiClient dataValidationClient,
        ExtractsDbContext extractsDbContext,
        IExtractRequests extractRequests,
        SqsJsonMessageSerializer sqsJsonMessageSerializer,
        IMediator mediator,
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
        _extractRequests = extractRequests;
        _sqsJsonMessageSerializer = sqsJsonMessageSerializer;
        _mediator = mediator;
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

        // if automatic validation was completed, then register
        //await _mediator.Send(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest, cancellationToken);

        return new object();
    }
}
