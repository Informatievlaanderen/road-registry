namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;
using ModifyRoadSegmentGeometry = BackOffice.Uploads.ModifyRoadSegmentGeometry;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentOutlineGeometrySqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        ILogger<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler> logger)
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

    protected override async Task<object> InnerHandle(ChangeRoadSegmentOutlineGeometrySqsLambdaRequest request, CancellationToken cancellationToken)
    {
        // Do NOT lock the stream store for stream RoadNetworks.Stream

        var roadSegmentId = request.Request.RoadSegmentId;

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Wijzig geometrie van ingeschetst wegsegment", async translatedChanges =>
        {
            var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

            var problems = Problems.None;

            var roadSegment = network.FindRoadSegment(roadSegmentId);
            if (roadSegment == null || roadSegment.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            {
                if (roadSegment == null)
                {
                    problems = problems.Add(new RoadSegmentOutlinedNotFound());
                    throw new RoadRegistryProblemsException(problems);
                }
                throw new RoadSegmentOutlinedNotFoundException();
            }

            var recordNumber = RecordNumber.Initial;

            var geometry = GeometryTranslator.Translate(request.Request.Geometry);
            var fromPosition = RoadSegmentPosition.Zero;
            var toPosition = RoadSegmentPosition.FromDouble(geometry.Length);

            var lanes = roadSegment.AttributeHash.Lanes
                .Select(lane => new RoadSegmentLaneAttribute(network.ProvidesNextRoadSegmentLaneAttributeId()(roadSegmentId)(), lane.Count, lane.Direction, fromPosition, toPosition))
                .ToList();
            var surfaces = roadSegment.AttributeHash.Surfaces
                .Select(surface => new RoadSegmentSurfaceAttribute(network.ProvidesNextRoadSegmentSurfaceAttributeId()(roadSegmentId)(), surface.Type, fromPosition, toPosition))
                .ToList();
            var widths = roadSegment.AttributeHash.Widths
                .Select(width => new RoadSegmentWidthAttribute(network.ProvidesNextRoadSegmentWidthAttributeId()(roadSegmentId)(), width.Width, fromPosition, toPosition))
                .ToList();
            
            return translatedChanges.AppendChange(new ModifyRoadSegmentGeometry(
                recordNumber,
                roadSegmentId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                request.Request.Geometry,
                lanes,
                surfaces,
                widths
            ));
        }, cancellationToken);

        var lastHash = await GetRoadSegmentHash(new RoadSegmentId(roadSegmentId), cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(ChangeRoadSegmentOutlineGeometrySqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
