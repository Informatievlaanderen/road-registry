namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CreateRoadSegmentOutline;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Exceptions;
using Hosts;
using Infrastructure;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.StreetName;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public sealed class CreateRoadSegmentOutlineV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<CreateRoadSegmentOutlineV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IOrganizationCache _organizationCache;
    private readonly IStreetNameClient _streetNameClient;
    private readonly ExtractsDbContext _extractsDbContext;

    private static readonly string[] ProposedOrCurrentStreetNameStatuses =
    [
        StreetNameStatus.Current,
        StreetNameStatus.Proposed
    ];

    public CreateRoadSegmentOutlineV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        Marten.IDocumentStore store,
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

    protected override async Task<object> InnerHandle(CreateRoadSegmentOutlineV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        var problems = Problems.None;

        var maintenanceAuthorityIds = sqsLambdaRequest.Request.MaintenanceAuthorityId
            .Select(x => x.MaintenanceAuthorityId)
            .Distinct()
            .ToArray();
        var maintenanceAuthorityIdMapping = new Dictionary<OrganizationId, OrganizationId>();
        foreach (var maintenanceAuthorityId in maintenanceAuthorityIds)
        {
            var (actualMaintenanceAuthorityId, maintenanceAuthorityProblems) = await FindOrganizationId(maintenanceAuthorityId, cancellationToken);
            problems += maintenanceAuthorityProblems;
            maintenanceAuthorityIdMapping.Add(maintenanceAuthorityId, actualMaintenanceAuthorityId);
        }

        var streetNameIds = sqsLambdaRequest.Request.StreetNameId
            .Select(x => x.StreetNameId)
            .Distinct()
            .ToArray();
        var streetNameIdProblemsCollection = await Task.WhenAll(streetNameIds.Select(streetNameId => ValidateStreetNameId(streetNameId, cancellationToken)));
        foreach (var streetNameIdProblems in streetNameIdProblemsCollection)
        {
            problems += streetNameIdProblems;
        }

        var roadSegmentAttributes = new RoadSegmentAttributes
        {
            AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>(
                sqsLambdaRequest.Request.AccessRestriction.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.AccessRestriction))),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(
                sqsLambdaRequest.Request.Category.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.Category))),
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(
                sqsLambdaRequest.Request.Morphology.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.Morphology))),
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(
                sqsLambdaRequest.Request.StreetNameId.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), x.Side, x.StreetNameId))),
            MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(
                sqsLambdaRequest.Request.MaintenanceAuthorityId.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), x.Side, maintenanceAuthorityIdMapping[x.MaintenanceAuthorityId]))),
            SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(
                sqsLambdaRequest.Request.SurfaceType.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.SurfaceType))),
            CarTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(
                sqsLambdaRequest.Request.CarTrafficDirection.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.TrafficDirection))),
            BikeTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(
                sqsLambdaRequest.Request.BikeTrafficDirection.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.TrafficDirection))),
            PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(
                sqsLambdaRequest.Request.PedestrianTrafficDirection.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.TrafficDirection))),
        };
        problems += new RoadSegmentAttributesValidator().Validate(roadSegmentAttributes, sqsLambdaRequest.Request.Geometry.Value.Length);

        //TODO-pr inwinning overlap check: geometry must be completely within inwinningszone
        // var geometry = sqsLambdaRequest.Request.Geometry.Value;
        // var temporaryId = new RoadSegmentId(1);
        // var overlapWithInwinningszone = (await _extractsDbContext.CheckWhichOverlapWithInwinningszone(
        //     [(geometry, temporaryId)], cancellationToken)).Any();
        // if (overlapWithInwinningszone)
        // {
        //     problems += new RoadSegmentOverlapsWithInwinningszone();
        // }

        if (problems.Any())
        {
            throw new RoadRegistryProblemsException(problems);
        }

        var roadSegment = RoadSegment.Create(new OutlinedRoadSegmentWasAdded
        {
            RoadSegmentId = await _roadNetworkIdGenerator.NewRoadSegmentIdAsync(),
            Geometry = sqsLambdaRequest.Request.Geometry,
            Status = sqsLambdaRequest.Request.Status,
            AccessRestriction = roadSegmentAttributes.AccessRestriction,
            Category = roadSegmentAttributes.Category,
            Morphology = roadSegmentAttributes.Morphology,
            StreetNameId = roadSegmentAttributes.StreetNameId,
            MaintenanceAuthorityId = roadSegmentAttributes.MaintenanceAuthorityId,
            SurfaceType = roadSegmentAttributes.SurfaceType,
            CarTrafficDirection = roadSegmentAttributes.CarTrafficDirection,
            BikeTrafficDirection = roadSegmentAttributes.BikeTrafficDirection,
            PedestrianTrafficDirection = roadSegmentAttributes.PedestrianTrafficDirection,
            Provenance = new ProvenanceData(sqsLambdaRequest.Provenance)
        });

        await _roadNetworkRepository.Save(new ScopedRoadNetwork(new ScopedRoadNetworkId(Guid.Empty), roadSegments: [roadSegment]), GetType().Name, cancellationToken);

        Logger.LogInformation("Created road segment {RoadSegmentId}", roadSegment.RoadSegmentId);

        return new ETagResponse(string.Format(GetRoadSegmentDetailUrlFormat(WellKnownPublicApiVersions.V3), roadSegment.RoadSegmentId), roadSegment.LastEventHash);
    }

    private async Task<(OrganizationId, Problems)> FindOrganizationId(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var problems = Problems.None;

        var maintenanceAuthorityOrganization = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(organizationId, cancellationToken);
        if (maintenanceAuthorityOrganization is not null)
        {
            return (maintenanceAuthorityOrganization.Code, problems);
        }

        if (OrganizationOvoCode.AcceptsValue(organizationId))
        {
            problems = problems.Add(new MaintenanceAuthorityNotKnown(organizationId));
        }

        return (organizationId, problems);
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
}
