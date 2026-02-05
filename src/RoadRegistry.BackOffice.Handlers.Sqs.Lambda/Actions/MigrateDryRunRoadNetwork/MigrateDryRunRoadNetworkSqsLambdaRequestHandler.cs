namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateDryRunRoadNetwork;

using System.Data;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Exceptions;
using Hosts;
using Infrastructure;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadNetwork;
using RoadRegistry.Infrastructure;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using TicketingService.Abstractions;

public sealed class MigrateDryRunRoadNetworkSqsLambdaRequestHandler : SqsLambdaHandler<MigrateDryRunRoadNetworkSqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IExtractRequests _extractRequests;
    private readonly IMediator _mediator;

    public MigrateDryRunRoadNetworkSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        IExtractRequests extractRequests,
        IMediator mediator,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory,
            TicketingBehavior.Error)
    {
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _extractRequests = extractRequests;
        _mediator = mediator;
    }

    protected override async Task<object> InnerHandle(MigrateDryRunRoadNetworkSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        await MigrateWithoutSaving(sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest, cancellationToken);

        await _mediator.Send(new DataValidationSqsRequest
        {
            TicketId = sqsLambdaRequest.TicketId,
            ProvenanceData = new ProvenanceData(sqsLambdaRequest.Provenance),
            MigrateRoadNetworkSqsRequest = sqsLambdaRequest.Request.MigrateRoadNetworkSqsRequest
        }, cancellationToken);

        return new object();
    }

    private async Task MigrateWithoutSaving(MigrateRoadNetworkSqsRequest command, CancellationToken cancellationToken)
    {
        var roadNetworkChanges = command.Changes.ToRoadNetworkChanges(command.ProvenanceData);

        var roadNetwork = await Load(roadNetworkChanges, new ScopedRoadNetworkId(command.DownloadId.ToGuid()));

        var changeResult = roadNetwork.Migrate(roadNetworkChanges, command.DownloadId, _roadNetworkIdGenerator);
        if (changeResult.Problems.HasError())
        {
            await _extractRequests.AutomaticValidationFailedAsync(command.UploadId, cancellationToken);

            throw new RoadRegistryProblemsException(changeResult.Problems);
        }
    }

    private async Task<ScopedRoadNetwork> Load(RoadNetworkChanges roadNetworkChanges, ScopedRoadNetworkId roadNetworkId)
    {
        if (!roadNetworkChanges.Any())
        {
            return new ScopedRoadNetwork(roadNetworkId);
        }

        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, roadNetworkChanges.BuildScopeGeometry(), ids: roadNetworkChanges.Ids, onlyV2: true);
        return await _roadNetworkRepository.Load(
            session,
            ids,
            roadNetworkId
        );
    }
}
