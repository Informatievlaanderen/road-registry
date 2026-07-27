namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentAttributes;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadSegmentAttributesV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;

    public ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
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
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var command = sqsLambdaRequest.Request;

        await Handle(command, cancellationToken);

        var roadSegmentIds = command.Groups
            .SelectMany(x => x.RoadSegmentIds)
            .Distinct()
            .ToList();

        var responses = new List<ETagResponse>();
        {
            await using var session = Store.LightweightSession();

            foreach (var roadSegmentId in roadSegmentIds)
            {
                var roadSegmentHash = await GetRoadSegmentHash(session, roadSegmentId, cancellationToken);
                responses.Add(new ETagResponse(string.Format(GetRoadSegmentDetailUrlFormat(WellKnownPublicApiVersions.V3), roadSegmentId), roadSegmentHash));
            }
        }

        return responses;
    }

    private Task Handle(ChangeRoadSegmentAttributesV2SqsRequest command, CancellationToken cancellationToken)
    {
        return Store.IdempotentSession(command, async session =>
        {
            var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);
            var roadSegmentIds = command.Groups.SelectMany(x => x.RoadSegmentIds).Distinct().ToList();
            var roadNetwork = await Load(session, roadSegmentIds, scopedRoadNetworkId);

            var changes = RoadNetworkChanges.Start().WithProvenance(command.ProvenanceData.ToProvenance());

            foreach (var group in command.Groups)
            {
                foreach (var roadSegmentId in group.RoadSegmentIds)
                {
                    // Resolve a null totPositie to the segment's own length; a null vanPositie to 0. A missing segment
                    // yields length 0 - the domain reports it as not found before any range validation runs.
                    var segmentLength = roadNetwork.RoadSegments.TryGetValue(roadSegmentId, out var roadSegment)
                        ? roadSegment.Geometry.Value.Length
                        : 0d;

                    changes = changes.Add(new ModifyRoadSegmentAttributesChange
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

            var result = roadNetwork.Change(changes, downloadId: null, _roadNetworkIdGenerator, Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);

            Logger.LogInformation("Changed attributes for road segments {RoadSegmentIds}", string.Join(",", roadSegmentIds));
        }, cancellationToken, Logger);
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
