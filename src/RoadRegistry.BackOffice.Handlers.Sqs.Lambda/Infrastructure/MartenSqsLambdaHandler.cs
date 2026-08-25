namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;

using Abstractions;
using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Exceptions;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;
using ValueObjects.Problems;
using ETag = Be.Vlaanderen.Basisregisters.Api.ETag.ETag;

public abstract class MartenSqsLambdaHandler<TSqsLambdaRequest> : RoadRegistryMartenSqsLambdaHandler<TSqsLambdaRequest>
    where TSqsLambdaRequest : SqsLambdaRequest
{
    protected MartenSqsLambdaHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        Marten.IDocumentStore store,
        ILoggerFactory loggerFactory,
        TicketingBehavior ticketingBehavior = TicketingBehavior.All,
        IProblemTranslator? problemTranslator = null)
        : base(options, retryPolicy, ticketing, idempotentCommandHandler, store, loggerFactory, ticketingBehavior, problemTranslator)
    {
    }

    protected async Task<string> GetRoadSegmentHash(
        RoadSegmentId roadSegmentId,
        CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        return await GetRoadSegmentHash(session, roadSegmentId, cancellationToken);
    }

    protected async Task<string> GetRoadSegmentHash(
        IDocumentSession session,
        RoadSegmentId roadSegmentId,
        CancellationToken cancellationToken)
    {
        var roadSegment = await session.LoadAsync(roadSegmentId, cancellationToken);
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        return roadSegment.LastEventHash;
    }

    // The summary is recovered from the persisted scoped road network aggregate (populated by the change-summary
    // event) rather than from the domain call, so a retry that skips the mutation still yields the same response.
    //
    // An action that turns out to change nothing - every road segment already had the value being asked for - records
    // no events at all, so the aggregate stream is never written and there is nothing to load back. That is a
    // successful no-op, not a failure, and it is answered with an empty summary.
    protected async Task<RoadNetworkChangesSummary> GetSummaryOfLastChange(
        ScopedRoadNetworkId scopedRoadNetworkId,
        CancellationToken cancellationToken)
    {
        await using var session = Store.LightweightSession();

        var scopedRoadNetwork = await session.LoadAsync(scopedRoadNetworkId, cancellationToken);

        return scopedRoadNetwork?.SummaryOfLastChange ?? new RoadNetworkChangesSummary();
    }

    protected override TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        return exception switch
        {
            RoadSegmentOutlinedNotFoundException => new RoadSegmentOutlinedNotFound().ToTicketError(WellKnownProblemTranslators.Default),
            RoadSegmentNotFoundException => new RoadSegmentNotFound().ToTicketError(WellKnownProblemTranslators.Default),
            ExtractRequestNotFoundException ex => new ExtractNotFound(ex.DownloadId).ToTicketError(WellKnownProblemTranslators.Default),
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
            cancellationToken);

        var lastHashTag = new ETag(ETagType.Strong, latestEventHash);

        if (request.IfMatchHeaderValue != lastHashTag.ToString())
        {
            throw new IfMatchHeaderValueMismatchException();
        }
    }
}
