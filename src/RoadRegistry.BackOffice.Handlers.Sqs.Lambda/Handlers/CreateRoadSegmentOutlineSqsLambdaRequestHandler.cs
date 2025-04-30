namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;
using AddRoadSegment = BackOffice.Uploads.AddRoadSegment;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class CreateRoadSegmentOutlineSqsLambdaRequestHandler : SqsLambdaHandler<CreateRoadSegmentOutlineSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly IOrganizationCache _organizationCache;

    public CreateRoadSegmentOutlineSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        IOrganizationCache organizationCache,
        ILogger<CreateRoadSegmentOutlineSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _organizationCache = organizationCache;
    }

    protected override async Task<object> InnerHandle(CreateRoadSegmentOutlineSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var startSw = Stopwatch.StartNew();

        var sw = Stopwatch.StartNew();
        var changeRoadNetworkCommand = await _changeRoadNetworkDispatcher.DispatchAsync(sqsLambdaRequest, "Wegsegment schetsen", async translatedChanges =>
        {
            sw.Restart();

            Logger.LogInformation("TIMETRACKING handler: loading RoadNetwork took {Elapsed}", sw.Elapsed);
            sw.Restart();

            var request = sqsLambdaRequest.Request;
            var recordNumber = RecordNumber.Initial;
            var problems = Problems.None;

            var geometry = GeometryTranslator.Translate(request.Geometry);

            var fromPosition = new RoadSegmentPosition(0);
            var toPosition = new RoadSegmentPosition((decimal)geometry.Length);

            var (maintenanceAuthority, maintenanceAuthorityProblems) = await FindOrganizationId(request.MaintenanceAuthority, cancellationToken);
            problems += maintenanceAuthorityProblems;

            translatedChanges = translatedChanges.AppendChange(
                new AddRoadSegment(
                        recordNumber,
                        new RoadSegmentId(1),
                        new RoadSegmentId(1),
                        RoadNodeId.Zero,
                        RoadNodeId.Zero,
                        maintenanceAuthority,
                        RoadSegmentGeometryDrawMethod.Outlined,
                        request.Morphology,
                        request.Status,
                        request.Category,
                        request.AccessRestriction,
                        null,
                        null)
                    .WithGeometry(geometry)
                    .WithSurface(new RoadSegmentSurfaceAttribute(AttributeId.Initial, request.SurfaceType, fromPosition, toPosition))
                    .WithWidth(new RoadSegmentWidthAttribute(AttributeId.Initial, request.Width, fromPosition, toPosition))
                    .WithLane(new RoadSegmentLaneAttribute(AttributeId.Initial, request.LaneCount, request.LaneDirection, fromPosition, toPosition))
            );

            if (problems.Any())
            {
                throw new RoadRegistryProblemsException(problems);
            }

            Logger.LogInformation("TIMETRACKING handler: converting request to TranslatedChanges took {Elapsed}", sw.Elapsed);
            return translatedChanges;
        }, cancellationToken);

        sw.Restart();
        var roadSegmentId = new RoadSegmentId(changeRoadNetworkCommand.Changes.Single().AddRoadSegment.PermanentId.Value);
        Logger.LogInformation("Created road segment {RoadSegmentId}", roadSegmentId);
        var lastHash = await GetRoadSegmentHash(roadSegmentId, RoadSegmentGeometryDrawMethod.Outlined, cancellationToken);
        Logger.LogInformation("TIMETRACKING handler: getting RoadSegment hash took {Elapsed}", sw.Elapsed);

        Logger.LogInformation("TIMETRACKING handler: entire handler took {Elapsed}", startSw.Elapsed);

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
