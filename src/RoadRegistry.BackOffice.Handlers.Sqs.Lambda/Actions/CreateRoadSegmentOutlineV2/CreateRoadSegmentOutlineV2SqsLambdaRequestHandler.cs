namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CreateRoadSegmentOutlineV2;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Infrastructure;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.StreetName;
using RoadRegistry.ValueObjects.Problems;
using TicketingService.Abstractions;

public sealed class CreateRoadSegmentOutlineV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<CreateRoadSegmentOutlineV2SqsLambdaRequest>
{
    private readonly IRoadNetworkRepository _roadNetworkRepository;
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
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
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
        _organizationCache = organizationCache;
        _streetNameClient = streetNameClient;
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(CreateRoadSegmentOutlineV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);
        var command = sqsLambdaRequest.Request;

        await Store.IdempotentSession(command, async session =>
        {
            var problems = Problems.None;

            var maintenanceAuthorityIds = command.MaintenanceAuthorityId
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

            var streetNameIds = command.StreetNameId
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
                    command.AccessRestriction.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.AccessRestriction))),
                Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(
                    command.Category.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.Category))),
                Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>(
                    command.Morphology.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.Morphology))),
                StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(
                    command.StreetNameId.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), x.Side, x.StreetNameId))),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(
                    command.MaintenanceAuthorityId.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), x.Side, maintenanceAuthorityIdMapping[x.MaintenanceAuthorityId]))),
                SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>(
                    command.SurfaceType.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.SurfaceType))),
                CarTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(
                    command.CarTrafficDirection.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.TrafficDirection))),
                BikeTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(
                    command.BikeTrafficDirection.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.TrafficDirection))),
                PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(
                    command.PedestrianTrafficDirection.Select(x => (new RoadSegmentPositionCoverage(x.FromPosition, x.ToPosition), RoadSegmentAttributeSide.Both, x.TrafficDirection))),
            };
            problems += new RoadSegmentAttributesValidator().Validate(roadSegmentAttributes, command.Geometry.Value.Length);

            var isCompletelyWithinCompletedInwinningszone = await _extractsDbContext.IsCompletelyWithinCompletedInwinningszone(command.Geometry.Value, cancellationToken);
            if (!isCompletelyWithinCompletedInwinningszone)
            {
                problems += new RoadSegmentOutsideCompletedInwinningszone();
            }

            if (problems.Any())
            {
                throw new RoadRegistryProblemsException(problems);
            }

            var roadSegment = RoadSegment.Create(new OutlinedRoadSegmentWasAdded
            {
                RoadSegmentId = command.RoadSegmentId,
                Geometry = command.Geometry,
                Status = command.Status,
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

            _roadNetworkRepository.Save(session, new ScopedRoadNetwork(new ScopedRoadNetworkId(Guid.NewGuid()), roadSegments: [roadSegment]), GetType().Name);

            Logger.LogInformation("Created outlined road segment {RoadSegmentId}", roadSegment.RoadSegmentId);
        }, cancellationToken, Logger);

        var roadSegmentHash = await GetRoadSegmentHash(command.RoadSegmentId, cancellationToken);
        return new ETagResponse(string.Format(GetRoadSegmentDetailUrlFormat(WellKnownPublicApiVersions.V3), command.RoadSegmentId), roadSegmentHash);
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
}
