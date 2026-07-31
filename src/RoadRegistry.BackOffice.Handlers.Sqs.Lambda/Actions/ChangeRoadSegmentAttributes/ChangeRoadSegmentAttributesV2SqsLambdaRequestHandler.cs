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

public sealed class ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<ChangeRoadSegmentAttributesV2SqsLambdaRequest>
{
    private static readonly string[] ProposedOrCurrentStreetNameStatuses =
    [
        StreetNameStatus.Current,
        StreetNameStatus.Proposed
    ];

    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IOrganizationCache _organizationCache;
    private readonly IStreetNameClient _streetNameClient;

    public ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IOrganizationCache organizationCache,
        IStreetNameClient streetNameClient,
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
        _organizationCache = organizationCache;
        _streetNameClient = streetNameClient;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var changeResultSummary = await Handle(sqsLambdaRequest.Request, cancellationToken);

        return new ChangeRoadNetworkTicketResult
        {
            Summary = new RoadNetworkChangedSummary(changeResultSummary)
        };
    }

    private async Task<RoadNetworkChangesSummary> Handle(ChangeRoadSegmentAttributesV2SqsRequest command, CancellationToken cancellationToken)
    {
        var scopedRoadNetworkId = new ScopedRoadNetworkId(command.TicketId);

        await Store.IdempotentSession(command, async session =>
        {
            var roadSegmentIds = command.Groups.SelectMany(x => x.RoadSegmentIds).Distinct().ToList();
            var roadNetwork = await Load(session, roadSegmentIds, scopedRoadNetworkId);

            // The street name and the maintenance authority live outside the road network, so they are validated here
            // before the domain is called: the street name against the street name registry and the maintenance
            // authority against the organization cache. Resolving the authority also maps an OVO code or KBO number
            // onto the organization code that is actually stored.
            var (maintenanceAuthorityIds, referenceProblems) = await ValidateReferences(command, cancellationToken);
            referenceProblems.ThrowIfError();

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
                        MaintenanceAuthorityId = BuildSidedValues(group.MaintenanceAuthority, segmentLength, maintenanceAuthorityIds),
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
        return scopedRoadNetwork.SummaryOfLastChange!;
    }

    // Validates every distinct street name and maintenance authority in the request and returns the resolved
    // organization codes, keyed by the code as it was given.
    private async Task<(IReadOnlyDictionary<OrganizationId, OrganizationId> MaintenanceAuthorityIds, Problems Problems)> ValidateReferences(
        ChangeRoadSegmentAttributesV2SqsRequest command, CancellationToken cancellationToken)
    {
        var problems = Problems.None;

        var streetNameIds = command.Groups
            .Where(x => x.StreetName is not null)
            .SelectMany(x => x.StreetName!)
            .Select(x => x.Value)
            .Where(x => x > 0)
            .Distinct()
            .ToArray();
        foreach (var streetNameProblems in await Task.WhenAll(streetNameIds.Select(x => ValidateStreetNameId(x, cancellationToken))))
        {
            problems += streetNameProblems;
        }

        var maintenanceAuthorityIds = new Dictionary<OrganizationId, OrganizationId>();
        var organizationIds = command.Groups
            .Where(x => x.MaintenanceAuthority is not null)
            .SelectMany(x => x.MaintenanceAuthority!)
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
