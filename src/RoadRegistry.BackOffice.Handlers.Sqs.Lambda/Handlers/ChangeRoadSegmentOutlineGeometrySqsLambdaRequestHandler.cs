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
using System.Linq;
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
        var roadSegmentId = request.Request.RoadSegmentId;

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Wijzig geometrie van ingeschetst wegsegment", async translatedChanges =>
        {
            var network = await RoadRegistryContext.RoadNetworks.ForOutlinedRoadSegment(roadSegmentId, cancellationToken);

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

            var attributeId = AttributeId.Initial;
            AttributeId GetNextAttributeId()
            {
                attributeId = attributeId.Next();
                return attributeId;
            }

            var lanes = roadSegment.Lanes
                .Select(lane => new RoadSegmentLaneAttribute(GetNextAttributeId(), lane.Count, lane.Direction, fromPosition, toPosition))
                .ToList();
            var surfaces = roadSegment.Surfaces
                .Select(surface => new RoadSegmentSurfaceAttribute(GetNextAttributeId(), surface.Type, fromPosition, toPosition))
                .ToList();
            var widths = roadSegment.Widths
                .Select(width => new RoadSegmentWidthAttribute(GetNextAttributeId(), width.Width, fromPosition, toPosition))
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

        var lastHash = await GetRoadSegmentHash(roadSegmentId, RoadSegmentGeometryDrawMethod.Outlined, cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(ChangeRoadSegmentOutlineGeometrySqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
