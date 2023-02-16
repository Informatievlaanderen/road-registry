namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Microsoft.Extensions.Logging;
using Hosts;
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
    
    protected override TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        return exception switch
        {
            RoadSegmentNotFoundException => new TicketError(
                ValidationErrors.RoadSegment.NotFound.Message,
                ValidationErrors.RoadSegment.NotFound.Code),
            _ => null
        };
    }

    protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not IHasRoadSegmentId id)
        {
            return;
        }

        var latestEventHash = await GetRoadSegmentHash(
            new RoadSegmentId(id.RoadSegmentId),
            cancellationToken);

        var lastHashTag = new ETag(ETagType.Strong, latestEventHash);

        if (request.IfMatchHeaderValue != lastHashTag.ToString())
        {
            throw new IfMatchHeaderValueMismatchException();
        }
    }
}
