namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentAttributes;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadSegmentAttributesV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;

    public ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            store,
            loggerFactory)
    {
        _roadNetworkRepository = roadNetworkRepository;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResult = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResult.Summary)
        };
    }

    private async Task<RoadNetworkChangeResult> Handle(ChangeRoadSegmentAttributesV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadSegmentIds = command.Groups.SelectMany(x => x.RoadSegmentIds).Distinct().ToList();
            var roadNetwork = await Load(session, roadSegmentIds, scopedRoadNetworkId);

            var provenance = command.ProvenanceData.ToProvenance();
            var changes = new List<ModifyRoadSegmentChange>();

            foreach (var group in command.Groups)
            {
                foreach (var roadSegmentId in group.RoadSegmentIds)
                {
                    // Resolve a null totPositie to the segment's own length; a null vanPositie to 0. A missing segment
                    // yields length 0 - the domain reports it as not found before any range validation runs.
                    var segmentLength = roadNetwork.RoadSegments.TryGetValue(roadSegmentId, out var roadSegment)
                        ? roadSegment.Geometry.Value.Length
                        : 0d;

                    // Attribute-only edit: reuse the generic road segment modification and leave geometry, draw
                    // method and status null so they stay untouched. The causation id identifies the action.
                    changes.Add(new ModifyRoadSegmentChange
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
                        Morphology = BuildValues(group.Morphology, segmentLength),
                        SurfaceType = BuildValues(group.SurfaceType, segmentLength),
                        AccessRestriction = BuildValues(group.AccessRestriction, segmentLength),
                        Category = BuildValues(group.Category, segmentLength),
                        StreetNameId = BuildSidedValues(group.StreetName, segmentLength),
                        MaintenanceAuthorityId = BuildSidedValues(group.MaintenanceAuthority, segmentLength),
                        CarTrafficDirection = BuildValues(group.CarTrafficDirection, segmentLength),
                        BikeTrafficDirection = BuildValues(group.BikeTrafficDirection, segmentLength),
                        PedestrianTrafficDirection = BuildValues(group.PedestrianTrafficDirection, segmentLength)
                    });
                }
            }

            var result = roadNetwork.ModifyRoadSegmentAttributes(changes, provenance, Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        // The summary is recovered from the persisted scoped road network aggregate (populated by the change-summary
        // event) rather than from the domain call, so a retry that skips the mutation still yields the same response.
        await using var readSession = Store.LightweightSession();
        var scopedRoadNetwork = await readSession.LoadAsync(scopedRoadNetworkId, cancellationToken);
        return new RoadNetworkChangeResult(Problems.None, scopedRoadNetwork.SummaryOfLastChange!);
    }

    private async Task<ScopedRoadNetwork> Load(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
    {
        var ids = await _roadNetworkRepository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], roadSegmentIds, [], []));
        return await _roadNetworkRepository.Load(session, ids, roadNetworkId);
    }

    private static RoadSegmentDynamicAttributeValues<T>? BuildValues<T>(IReadOnlyList<AttributeValue<T>>? source, double segmentLength)
        where T : notnull
    {
        if (source is null)
        {
            return null;
        }

        var values = new RoadSegmentDynamicAttributeValues<T>();
        foreach (var value in source)
        {
            values.Add(
                value.FromPosition ?? RoadSegmentPositionV2.Zero,
                value.ToPosition ?? new RoadSegmentPositionV2(segmentLength),
                value.Value);
        }
        return values;
    }

    private static RoadSegmentDynamicAttributeValues<T>? BuildSidedValues<T>(IReadOnlyList<SidedAttributeValue<T>>? source, double segmentLength)
        where T : notnull
    {
        if (source is null)
        {
            return null;
        }

        var values = new RoadSegmentDynamicAttributeValues<T>();
        foreach (var value in source)
        {
            values.Add(
                value.FromPosition ?? RoadSegmentPositionV2.Zero,
                value.ToPosition ?? new RoadSegmentPositionV2(segmentLength),
                value.Side,
                value.Value);
        }
        return values;
    }
}
