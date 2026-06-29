namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CreateRoadSegmentOutline;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
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
using TicketingService.Abstractions;
using ValueObjects.Problems;

public sealed class CreateRoadSegmentOutlineV2SqsLambdaRequestHandler : MartenSqsLambdaHandler<CreateRoadSegmentOutlineV2SqsLambdaRequest>
{
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly IOrganizationCache _organizationCache;
    private readonly ExtractsDbContext _extractsDbContext;

    public CreateRoadSegmentOutlineV2SqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        Marten.IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        IRoadNetworkIdGenerator roadNetworkIdGenerator,
        IOrganizationCache organizationCache,
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
        _store = store;
        _roadNetworkRepository = roadNetworkRepository;
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _organizationCache = organizationCache;
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<object> InnerHandle(CreateRoadSegmentOutlineV2SqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        using var _ = Logger.TimeAction(GetType().Name);

        await using var session = _store.LightweightSession();

        var roadSegmentAttributes = new RoadSegmentAttributes
        {
            // AccessRestriction = sqsLambdaRequest.Request.AccessRestriction,
            // Category = sqsLambdaRequest.Request.xxx,
            // Morphology = sqsLambdaRequest.Request.xxx,
            // StreetNameId = sqsLambdaRequest.Request.xxx,
            // MaintenanceAuthorityId = sqsLambdaRequest.Request.xxx,
            // SurfaceType = sqsLambdaRequest.Request.xxx,
            // CarTrafficDirection = sqsLambdaRequest.Request.xxx,
            // BikeTrafficDirection = sqsLambdaRequest.Request.xxx,
            // PedestrianTrafficDirection = sqsLambdaRequest.Request.xxx,
        };

        var problems = new RoadSegmentAttributesValidator().Validate(roadSegmentAttributes, sqsLambdaRequest.Request.Geometry.Value.Length);

        /*
        var (maintenanceAuthority, maintenanceAuthorityProblems) = await FindOrganizationId(request.MaintenanceAuthority, cancellationToken);
        problems += maintenanceAuthorityProblems;

        var temporaryId = new RoadSegmentId(1);
        var overlapWithInwinningszone = (await _extractsDbContext.CheckWhichOverlapWithInwinningszone(
            [(geometry, temporaryId)], cancellationToken)).Any();
        if (overlapWithInwinningszone)
        {
            problems += new RoadSegmentOverlapsWithInwinningszone();
        }*/

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
        var roadNetwork = new ScopedRoadNetwork(new ScopedRoadNetworkId(Guid.NewGuid()),
            roadSegments: [roadSegment]);

        await _roadNetworkRepository.Save(roadNetwork, GetType().Name, cancellationToken);

        var roadSegmentId = roadSegment.RoadSegmentId;
        Logger.LogInformation("Created road segment {RoadSegmentId}", roadSegmentId);
        var lastHash = await GetRoadSegmentHash(session, roadSegmentId, cancellationToken);

        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
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
}
