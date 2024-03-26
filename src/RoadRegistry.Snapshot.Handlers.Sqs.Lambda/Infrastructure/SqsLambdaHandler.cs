namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Infrastructure;

using BackOffice;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Hosts;
using Microsoft.Extensions.Logging;
using TicketingService.Abstractions;

public abstract class SqsLambdaHandler<TSqsLambdaRequest> : RoadRegistrySqsLambdaHandler<TSqsLambdaRequest>
    where TSqsLambdaRequest : SqsLambdaRequest
{
    protected SqsLambdaHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ILogger logger)
        : base(options, retryPolicy, ticketing, idempotentCommandHandler, roadRegistryContext, logger)
    {
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        return exception switch
        {
            _ => null
        };
    }

    protected override Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
