namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Editor.Projections;
using Editor.Schema;
using Editor.Schema.Extensions;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using RoadRegistry.BackOffice.Abstractions.Organizations;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using TicketingService.Abstractions;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;

public sealed class ChangeRoadSegmentAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentAttributesSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly IOrganizationRepository _organizationRepository;

    public ChangeRoadSegmentAttributesSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        IOrganizationRepository organizationRepository,
        ILogger<ChangeRoadSegmentAttributesSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _editorContext = editorContext;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _organizationRepository = organizationRepository;
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        // Do NOT lock the stream store for stream RoadNetworks.Stream

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Attributen wijzigen", async translatedChanges =>
        {
            var recordNumber = RecordNumber.Initial;
            var problems = Problems.None;

            foreach (var change in request.Request.ChangeRequests)
            {
                var roadSegmentId = new RoadSegmentId(change.Id);

                var editorRoadSegment = await _editorContext.RoadSegments.SingleOrDefaultIncludingLocalAsync(x => x.Id == change.Id, cancellationToken);
                if (editorRoadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                    continue;
                }

                var roadSegmentDbaseRecord = new RoadSegmentDbaseRecord().FromBytes(editorRoadSegment.DbaseRecord, _manager, _fileEncoding);
                var geometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentDbaseRecord.METHODE.Value];
                var streamName = RoadNetworkStreamNameProvider.Get(roadSegmentId, geometryDrawMethod);

                var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(streamName, cancellationToken);

                var networkRoadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
                if (networkRoadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                    continue;
                }

                var maintenanceAuthority = change.MaintenanceAuthority;
                if (maintenanceAuthority is not null)
                {
                    (maintenanceAuthority, var maintenanceAuthorityProblems) = await FindOrganizationId(maintenanceAuthority.Value, cancellationToken);
                    problems += maintenanceAuthorityProblems;
                }

                translatedChanges = translatedChanges.AppendChange(new ModifyRoadSegmentAttributes(recordNumber, roadSegmentId, geometryDrawMethod)
                {
                    MaintenanceAuthority = maintenanceAuthority,
                    Morphology = change.Morphology,
                    Status = change.Status,
                    Category = change.Category,
                    AccessRestriction = change.AccessRestriction
                });

                recordNumber = recordNumber.Next();
            }

            if (problems.Any())
            {
                throw new RoadRegistryProblemsException(problems);
            }

            return translatedChanges;
        }, cancellationToken);

        return new ChangeRoadSegmentAttributesResponse();
    }

    private async Task<(OrganizationId, Problems)> FindOrganizationId(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var problems = Problems.None;
        
        var maintenanceAuthorityOrganization = await _organizationRepository.FindByIdOrOvoCodeAsync(organizationId, cancellationToken);
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
