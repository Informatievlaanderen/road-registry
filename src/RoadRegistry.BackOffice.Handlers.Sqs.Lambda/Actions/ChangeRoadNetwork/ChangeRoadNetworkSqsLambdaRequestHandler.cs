namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class ChangeRoadNetworkSqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadNetworkSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IExtractUploadFailedEmailClient _extractUploadFailedEmailClient;

    public ChangeRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        ExtractsDbContext extractsDbContext,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            store,
            loggerFactory,
            TicketingBehavior.Complete | TicketingBehavior.Error,
            problemTranslator: WellKnownProblemTranslators.Extract)
    {
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractsDbContext = extractsDbContext;
        _extractUploadFailedEmailClient = extractUploadFailedEmailClient;
    }

    protected override async Task<object> InnerHandle(ChangeRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResult.Summary)
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(ChangeRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.DownloadId.ToGuid());

        await Store.IdempotentSession(command, async session =>
        {
            var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

            var roadNetwork = await Load(session, roadNetworkChanges, scopedRoadNetworkId);

            await ChangeRoadNetwork(session, command, roadNetwork, roadNetworkChanges, cancellationToken);
        }, cancellationToken, Logger);

        {
            await using var session = Store.LightweightSession();

            var roadNetwork = await session.LoadAsync(scopedRoadNetworkId, cancellationToken);
            var changeResult = new RoadNetworkChangeResult(Problems.None, roadNetwork.SummaryOfLastChange ?? new RoadNetworkChangesSummary());

            if (!await _extractsDbContext.IsUploadAcceptedAsync(command.UploadId, cancellationToken))
            {
                _extractsDbContext.InwinningRoadSegments.AddRange(changeResult.Summary.RoadSegments.Added.Select(roadSegmentId => new InwinningRoadSegment
                {
                    NisCode = null,
                    RoadSegmentId = roadSegmentId,
                    Completed = true
                }));
                await _extractsDbContext.UploadAcceptedAsync(command.UploadId, cancellationToken);
            }

            return changeResult;
        }
    }

    private async Task ChangeRoadNetwork(IDocumentSession session, ChangeRoadNetworkSqsRequest command, ScopedRoadNetwork roadNetwork, RoadNetworkChanges roadNetworkChanges, CancellationToken cancellationToken)
    {
        var changeResult = roadNetwork.Change(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator);
        if (changeResult.Problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

            if (command.SendFailedEmail)
            {
                await _extractUploadFailedEmailClient.SendAsync(new(command.DownloadId, command.ProvenanceData.Reason), cancellationToken);
            }

            throw new RoadRegistryProblemsException(changeResult.Problems);
        }

        _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkChanges roadNetworkChanges, ScopedRoadNetworkId roadNetworkId)
    {
        if (!roadNetworkChanges.Any())
        {
            return new ScopedRoadNetwork(roadNetworkId);
        }

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry(), ids: roadNetworkChanges.Ids);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }
}
