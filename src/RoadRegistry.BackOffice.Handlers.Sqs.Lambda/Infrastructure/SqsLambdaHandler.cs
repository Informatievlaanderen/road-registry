namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Abstractions;
using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Core;
using Hosts;
using Hosts.Infrastructure.Extensions;
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

    protected async Task<string> GetRoadSegmentHash(
        RoadSegmentId roadSegmentId,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        CancellationToken cancellationToken)
    {
        var streamName = RoadNetworkStreamNameProvider.Get(roadSegmentId, geometryDrawMethod);
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(streamName, cancellationToken);
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
            RoadSegmentOutlinedNotFoundException => new RoadSegmentOutlinedNotFound().ToTicketError(),
            RoadSegmentNotFoundException => new RoadSegmentNotFound().ToTicketError(),
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
            id.RoadSegmentId,
            id.GeometryDrawMethod,
            cancellationToken);

        var lastHashTag = new ETag(ETagType.Strong, latestEventHash);

        if (request.IfMatchHeaderValue != lastHashTag.ToString())
        {
            throw new IfMatchHeaderValueMismatchException();
        }
    }
}
