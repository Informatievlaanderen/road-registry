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

        await using var session = Store.LightweightSession();

        var latestEventHash = await GetRoadSegmentHash(
            session,
            id.RoadSegmentId,
            cancellationToken);

        var lastHashTag = new ETag(ETagType.Strong, latestEventHash);

        if (request.IfMatchHeaderValue != lastHashTag.ToString())
        {
            throw new IfMatchHeaderValueMismatchException();
        }
    }
}
