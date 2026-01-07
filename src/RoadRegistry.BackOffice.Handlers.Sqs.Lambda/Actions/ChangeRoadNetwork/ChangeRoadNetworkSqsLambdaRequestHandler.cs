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
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNetwork.Events.V2;
using RoadRegistry.RoadNetwork.ValueObjects;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public sealed class ChangeRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadNetworkSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IExtractRequests _extractRequests;
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
        IExtractRequests extractRequests,
        IExtractUploadFailedEmailClient extractUploadFailedEmailClient,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Complete | TicketingBehavior.Error)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractRequests = extractRequests;
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

        var roadNetwork = await Load(roadNetworkChanges, new RoadNetworkId(command.DownloadId));

        //TODO-pr add test wnr Summary is ingevuld dat de roadnetwork niet mag worden opgeslagen
        var changeResult = roadNetwork.SummaryOfLastChange is null
            ? await ChangeRoadNetwork(command, roadNetwork, roadNetworkChanges, cancellationToken)
            : new RoadNetworkChangeResult(Problems.None, roadNetwork.SummaryOfLastChange);

        await _extractRequests.UploadAcceptedAsync(command.DownloadId, cancellationToken);

        return changeResult;
    }

    private async Task<RoadNetworkChangeResult> ChangeRoadNetwork(ChangeRoadNetworkSqsRequest command, RoadNetwork roadNetwork, RoadNetworkChanges roadNetworkChanges, CancellationToken cancellationToken)
    {
        var changeResult = roadNetwork.Change(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator);
        if (changeResult.Problems.HasError())
        {
            await _extractRequests.UploadRejectedAsync(command.DownloadId, cancellationToken);

            if (command.SendFailedEmail)
            {
                await _extractUploadFailedEmailClient.SendAsync(new (command.DownloadId, command.ProvenanceData.Reason), cancellationToken);
            }

            throw new RoadRegistryProblemsException(changeResult.Problems);
        }

        await _roadNetworkRepository.Save(roadNetwork, command.GetType().Name, cancellationToken);
        return changeResult;
    }

    private async Task<RoadNetwork> Load(RoadNetworkChanges roadNetworkChanges, RoadNetworkId roadNetworkId)
    {
        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry());
        return await _roadNetworkRepository.Load(
            session,
            new RoadNetworkIds(
                roadNetworkChanges.RoadNodeIds.Union(ids.RoadNodeIds).ToList(),
                roadNetworkChanges.RoadSegmentIds.Union(ids.RoadSegmentIds).ToList(),
                roadNetworkChanges.GradeSeparatedJunctionIds.Union(ids.GradeSeparatedJunctionIds).ToList()
            ),
            roadNetworkId
        );
    }
}
