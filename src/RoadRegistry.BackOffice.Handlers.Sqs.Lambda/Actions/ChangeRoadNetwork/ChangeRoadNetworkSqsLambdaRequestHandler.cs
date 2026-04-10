namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Exceptions;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.DutchTranslations;
using ScopedRoadNetwork;
using ScopedRoadNetwork.Events.V2;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public sealed class ChangeRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadNetworkSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IExtractUploadFailedEmailClient _extractUploadFailedEmailClient;

    public ChangeRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
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
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Complete | TicketingBehavior.Error,
            problemTranslator: WellKnownProblemTranslators.Extract)
    {
        _store = store;
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
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

        var roadNetwork = await Load(roadNetworkChanges, new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        var changeResult = roadNetwork.SummaryOfLastChange is null
            ? await ChangeRoadNetwork(command, roadNetwork, roadNetworkChanges, cancellationToken)
            : new RoadNetworkChangeResult(Problems.None, roadNetwork.SummaryOfLastChange);

        _extractsDbContext.InwinningRoadSegments.AddRange(changeResult.Summary.RoadSegments.Added
            .Select(roadSegmentId => new InwinningRoadSegment
            {
                RoadSegmentId = roadSegmentId,
                Completed = true
            }));
        await _extractsDbContext.UploadAcceptedAsync(command.UploadId, cancellationToken);

        return changeResult;
    }

    private async Task<RoadNetworkChangeResult> ChangeRoadNetwork(ChangeRoadNetworkSqsRequest command, ScopedRoadNetwork roadNetwork, RoadNetworkChanges roadNetworkChanges, CancellationToken cancellationToken)
    {
        var changeResult = roadNetwork.Change(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator);
        if (changeResult.Problems.HasError())
        {
            await _extractsDbContext.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

            if (command.SendFailedEmail)
            {
                await _extractUploadFailedEmailClient.SendAsync(new (command.DownloadId, command.ProvenanceData.Reason), cancellationToken);
            }

            throw new RoadRegistryProblemsException(changeResult.Problems);
        }

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        return changeResult;
    }

    private async Task<ScopedRoadNetwork> Load(RoadNetworkChanges roadNetworkChanges, ScopedRoadNetworkId roadNetworkId)
    {
        if (!roadNetworkChanges.Any())
        {
            return new ScopedRoadNetwork(roadNetworkId);
        }

        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry(), ids: roadNetworkChanges.Ids);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }
}
