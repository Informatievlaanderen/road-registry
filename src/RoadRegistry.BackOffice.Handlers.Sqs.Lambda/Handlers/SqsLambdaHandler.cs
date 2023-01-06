namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
    using Microsoft.Extensions.Configuration;
    using RoadRegistry.BackOffice.Abstractions.Exceptions;
    using RoadRegistry.BackOffice.Abstractions.Validation;
    using TicketingService.Abstractions;

    public abstract class SqsLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
        where TSqsLambdaRequest : SqsLambdaRequest
    {
        protected IRoadRegistryContext RoadRegistryContext { get; }
        protected string DetailUrlFormat { get; }

        protected SqsLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler,
            IRoadRegistryContext roadRegistryContext)
            : base(retryPolicy, ticketing, idempotentCommandHandler)
        {
            RoadRegistryContext = roadRegistryContext;
            DetailUrlFormat = configuration.GetRequiredValue<string>("DetailUrl");
        }

        protected async Task<string> GetRoadSegmentHash(
            RoadSegmentId roadSegmentId,
            CancellationToken cancellationToken)
        {
            var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
            var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
            if (roadSegment == null)
            {
                throw new RoadSegmentNotFoundException();
            }

            return roadSegment.LastEventHash;
        }

        protected override async Task HandleAggregateIdIsNotFoundException(TSqsLambdaRequest request, CancellationToken cancellationToken)
        {
            await Ticketing.Error(request.TicketId,
                new TicketError(
                    ValidationErrors.RoadSegment.NotFound.Message,
                    ValidationErrors.RoadSegment.NotFound.Code),
                cancellationToken);
        }

        protected abstract TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request);

        protected override TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request)
        {
            var error = InnerMapDomainException(exception, request);
            if (error is not null)
            {
                return error;
            }

            return exception switch
            {
                RoadSegmentNotFoundException => new TicketError(
                    ValidationErrors.RoadSegment.NotFound.Message,
                    ValidationErrors.RoadSegment.NotFound.Code),
                _ => null
            };
        }
    }
}
