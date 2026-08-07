namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometry;

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
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.StreetName;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentGeometryV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadSegmentGeometryV2SqsLambdaRequest>
{
    private static readonly string[] ProposedOrCurrentStreetNameStatuses =
    [
        StreetNameStatus.Current,
        StreetNameStatus.Proposed
    ];

    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IOrganizationCache _organizationCache;
    private readonly IStreetNameClient _streetNameClient;
    private readonly ExtractsDbContext _extractsDbContext;

    public ChangeRoadSegmentGeometryV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        IOrganizationCache organizationCache,
        IStreetNameClient streetNameClient,
        ExtractsDbContext extractsDbContext,
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
        _organizationCache = organizationCache;
        _streetNameClient = streetNameClient;
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentGeometryV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeRoadSegmentGeometryV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var problems = Problems.None;

            // Road nodes are sticky, so the segments connected to this one through its start and end node are part of
            // the change and have to be in scope even though the request does not mention them.
            var ids = await _roadNetworkRepository.GetUnderlyingIdsWithConnectedSegments(session, [command.RoadSegmentId]);
            var roadNetwork = await _roadNetworkRepository.Load(session, ids, scopedRoadNetworkId);

            // The street name and the maintenance authority live outside the road network, so they are validated here
            // before the domain is called: the street name against the street name registry and the maintenance
            // authority against the organization cache. Resolving the authority also maps an OVO code or KBO number
            // onto the organization code that is actually stored.
            var (maintenanceAuthorityIds, referenceProblems) = await ValidateReferences(command, cancellationToken);
            problems += referenceProblems;

            var isCompletelyWithinCompletedInwinningszone = await _extractsDbContext.IsCompletelyWithinCompletedInwinningszone(command.Geometry.Value, cancellationToken);
            if (!isCompletelyWithinCompletedInwinningszone)
            {
                problems += new RoadSegmentOutsideCompletedInwinningszone();
            }

            problems.ThrowIfError();

            // The attribute positions apply to the geometry being submitted, not to the one on record, so an omitted
            // totPositie resolves against the new length.
            var segmentLength = command.Geometry.Value.Length;

            var change = new ModifyRoadSegmentGeometryChange
            {
                RoadSegmentId = command.RoadSegmentId,
                Geometry = command.Geometry,
                Morphology = BuildValues(command.Morphology, segmentLength),
                SurfaceType = BuildValues(command.SurfaceType, segmentLength),
                AccessRestriction = BuildValues(command.AccessRestriction, segmentLength),
                Category = BuildValues(command.Category, segmentLength),
                StreetNameId = BuildSidedValues(command.StreetName, segmentLength),
                MaintenanceAuthorityId = BuildSidedValues(command.MaintenanceAuthority, segmentLength, maintenanceAuthorityIds),
                CarTrafficDirection = BuildValues(command.CarTrafficDirection, segmentLength),
                BikeTrafficDirection = BuildValues(command.BikeTrafficDirection, segmentLength),
                PedestrianTrafficDirection = BuildValues(command.PedestrianTrafficDirection, segmentLength)
            };

            var result = roadNetwork.ModifyRoadSegmentGeometry(
                change,
                command.MayModifyMeasuredRoadSegments,
                _roadNetworkIdGenerator,
                command.ProvenanceData.ToProvenance(),
                Logger);
            result.Problems.ThrowIfError();

            _roadNetworkRepository.Save(session, roadNetwork, command.GetType().Name);
        }, cancellationToken, Logger);

        // The summary is recovered from the persisted scoped road network aggregate (populated by the change-summary
        // event) rather than from the domain call, so a retry that skips the mutation still yields the same response.
        await using var readSession = Store.LightweightSession();
        var scopedRoadNetwork = await readSession.LoadAsync(scopedRoadNetworkId, cancellationToken);
        return scopedRoadNetwork.SummaryOfLastChange!;
    }

    // Validates every distinct street name and maintenance authority in the request and returns the resolved
    // organization codes, keyed by the code as it was given.
    private async Task<(IReadOnlyDictionary<OrganizationId, OrganizationId> MaintenanceAuthorityIds, Problems Problems)> ValidateReferences(
        ChangeRoadSegmentGeometryV2SqsRequest command, CancellationToken cancellationToken)
    {
        var problems = Problems.None;

        var streetNameIds = (command.StreetName ?? [])
            .Select(x => x.Value)
            .Where(x => x > 0)
            .Distinct()
            .ToArray();
        foreach (var streetNameProblems in await Task.WhenAll(streetNameIds.Select(x => ValidateStreetNameId(x, cancellationToken))))
        {
            problems += streetNameProblems;
        }

        var maintenanceAuthorityIds = new Dictionary<OrganizationId, OrganizationId>();
        var organizationIds = (command.MaintenanceAuthority ?? [])
            .Select(x => x.Value)
            .Distinct()
            .ToArray();
        foreach (var organizationId in organizationIds)
        {
            var (actualOrganizationId, organizationProblems) = await FindOrganizationId(organizationId, cancellationToken);
            problems += organizationProblems;
            maintenanceAuthorityIds.Add(organizationId, actualOrganizationId);
        }

        return (maintenanceAuthorityIds, problems);
    }

    private async Task<(OrganizationId, Problems)> FindOrganizationId(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var maintenanceAuthorityOrganization = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(organizationId, cancellationToken);
        if (maintenanceAuthorityOrganization is not null)
        {
            return (maintenanceAuthorityOrganization.Code, Problems.None);
        }

        return (organizationId, Problems.Single(new MaintenanceAuthorityNotKnown(organizationId)));
    }

    private async Task<Problems> ValidateStreetNameId(StreetNameLocalId streetNameId, CancellationToken cancellationToken)
    {
        try
        {
            var streetName = await _streetNameClient.GetAsync(streetNameId, cancellationToken);
            if (streetName is null)
            {
                return Problems.Single(new StreetNameNotFound());
            }

            if (ProposedOrCurrentStreetNameStatuses.All(status => !string.Equals(streetName.Status, status, StringComparison.InvariantCultureIgnoreCase)))
            {
                return Problems.Single(new RoadSegmentStreetNameNotProposedOrCurrent());
            }
        }
        catch (StreetNameRegistryUnexpectedStatusCodeException ex)
        {
            Logger.LogError(ex.Message);

            return Problems.Single(new StreetNameRegistryUnexpectedError((int)ex.StatusCode));
        }

        return Problems.None;
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
        return BuildSidedValues(source, segmentLength, x => x);
    }

    // The maintenance authority is stored as the organization code, so the value given in the request (which can also
    // be an OVO code or a KBO number) is replaced by the code it resolved to.
    private static RoadSegmentDynamicAttributeValues<OrganizationId>? BuildSidedValues(
        IReadOnlyList<SidedAttributeValue<OrganizationId>>? source,
        double segmentLength,
        IReadOnlyDictionary<OrganizationId, OrganizationId> maintenanceAuthorityIds)
    {
        return BuildSidedValues(source, segmentLength, x => maintenanceAuthorityIds[x]);
    }

    private static RoadSegmentDynamicAttributeValues<T>? BuildSidedValues<T>(IReadOnlyList<SidedAttributeValue<T>>? source, double segmentLength, Func<T, T> selectValue)
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
                selectValue(value.Value));
        }
        return values;
    }
}
