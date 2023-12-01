namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using System.Diagnostics;
using TicketingService.Abstractions;
using AddRoadSegment = BackOffice.Uploads.AddRoadSegment;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class CreateRoadSegmentOutlineSqsLambdaRequestHandler : SqsLambdaHandler<CreateRoadSegmentOutlineSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public CreateRoadSegmentOutlineSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
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
    }

    protected override async Task<object> InnerHandle(CreateRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var startSw = Stopwatch.StartNew();

        var sw = Stopwatch.StartNew();
        var changeRoadNetworkCommand = await _changeRoadNetworkDispatcher.DispatchAsync(request, "Wegsegment schetsen", async translatedChanges =>
        {
            sw.Restart();

            Logger.LogInformation("TIMETRACKING handler: loading RoadNetwork took {Elapsed}", sw.Elapsed);
            sw.Restart();

            var r = request.Request;
            var recordNumber = RecordNumber.Initial;

            var geometry = GeometryTranslator.Translate(r.Geometry);

            var fromPosition = new RoadSegmentPosition(0);
            var toPosition = new RoadSegmentPosition((decimal)geometry.Length);

            translatedChanges = translatedChanges.AppendChange(
                new AddRoadSegment(
                        recordNumber,
                        new RoadSegmentId(1),
                        new RoadSegmentId(1),
                        r.MaintenanceAuthority,
                        RoadSegmentGeometryDrawMethod.Outlined,
                        r.Morphology,
                        r.Status,
                        RoadSegmentCategory.Unknown,
                        r.AccessRestriction)
                    .WithGeometry(geometry)
                    .WithSurface(new RoadSegmentSurfaceAttribute(AttributeId.Initial, r.SurfaceType, fromPosition, toPosition))
                    .WithWidth(new RoadSegmentWidthAttribute(AttributeId.Initial, r.Width, fromPosition, toPosition))
                    .WithLane(new RoadSegmentLaneAttribute(AttributeId.Initial, r.LaneCount, r.LaneDirection, fromPosition, toPosition))
            );

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
}
