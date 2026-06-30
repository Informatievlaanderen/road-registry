namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using ChangeRoadNetwork;
using Exceptions;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Infrastructure.MartenDb;
using ScopedRoadNetwork;
using ScopedRoadNetwork.Events.V2;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public sealed class MigrateRoadNetworkSqsLambdaRequestHandler : MartenSqsLambdaHandler<MigrateRoadNetworkSqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;

    public MigrateRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        Marten.IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        ExtractsDbContext extractsDbContext,
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
    }

    protected override async Task<object> InnerHandle(MigrateRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        try
        {
            var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

            return new ChangeRoadNetworkTicketResult
            {
                Summary = new RoadNetworkChangedSummary(changeResult.Summary)
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, $"Error while migrating roadnetwork with download id {sqsLambdaRequest.Request.DownloadId}");
            throw;
        }
    }

    private async Task<RoadNetworkChangeResult> Handle(MigrateRoadNetworkSqsRequest command, CancellationToken cancellationToken)
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
            var changeResult = new RoadNetworkChangeResult(Problems.None, roadNetwork.SummaryOfLastChange!);

            if (!await _extractsDbContext.IsUploadAcceptedAsync(command.UploadId, cancellationToken))
            {
                await CompleteInwinningStatuses(command.DownloadId, changeResult, cancellationToken);
                await _extractsDbContext.UploadAcceptedAsync(command.UploadId, cancellationToken);
            }

            return changeResult;
        }
    }

    private async Task ChangeRoadNetwork(IDocumentSession session, MigrateRoadNetworkSqsRequest command, ScopedRoadNetwork roadNetwork, RoadNetworkChanges roadNetworkChanges, CancellationToken cancellationToken)
    {
        var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator, Logger);
        if (changeResult.Problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

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

    private async Task CompleteInwinningStatuses(DownloadId downloadId, RoadNetworkChangeResult changeResult, CancellationToken cancellationToken)
    {
        var inwinningszone = await EntityFrameworkQueryableExtensions.SingleAsync(_extractsDbContext.Inwinningszones, x => x.DownloadId == downloadId.ToGuid(), cancellationToken);
        inwinningszone.Completed = true;

        var inwinningRoadSegments = await EntityFrameworkQueryableExtensions.ToListAsync(_extractsDbContext.InwinningRoadSegments
            .Where(x => x.NisCode == inwinningszone.NisCode), cancellationToken);
        foreach (var inwinningRoadSegment in inwinningRoadSegments)
        {
            inwinningRoadSegment.Completed = true;
        }

        _extractsDbContext.InwinningRoadSegments.AddRange(changeResult.Summary.RoadSegments.Added.Select(roadSegmentId => new InwinningRoadSegment
        {
            NisCode = inwinningszone.NisCode,
            RoadSegmentId = roadSegmentId,
            Completed = true
        }));
    }
}
