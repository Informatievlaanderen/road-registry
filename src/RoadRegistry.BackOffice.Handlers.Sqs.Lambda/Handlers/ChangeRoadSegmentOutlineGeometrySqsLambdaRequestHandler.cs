namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Requests;
using RoadNetwork;
using RoadSegment;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
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
            if (roadSegment is null || roadSegment.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            {
                if (roadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentOutlinedNotFound());
                    throw new RoadRegistryProblemsException(problems);
                }

                throw new RoadSegmentOutlinedNotFoundException();
            }

            var recordNumber = RecordNumber.Initial;

            var geometry = GeometryTranslator.Translate(request.Request.Geometry);
            problems += geometry.GetSingleLineString().GetProblemsForRoadSegmentOutlinedGeometry(roadSegmentId, VerificationContextTolerances.Default);
            if (problems.Any())
            {
                throw new RoadRegistryProblemsException(problems);
            }

            var fromPosition = RoadSegmentPosition.Zero;
            var toPosition = RoadSegmentPosition.FromDouble(geometry.Length);

            var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

            var lane = roadSegment.Lanes
                .Select(lane => new RoadSegmentLaneAttribute(attributeIdProvider.Next(), lane.Count, lane.Direction, fromPosition, toPosition))
                .Single();
            var surface = roadSegment.Surfaces
                .Select(surface => new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), surface.Type, fromPosition, toPosition))
                .Single();
            var width = roadSegment.Widths
                .Select(width => new RoadSegmentWidthAttribute(attributeIdProvider.Next(), width.Width, fromPosition, toPosition))
                .Single();

            return translatedChanges.AppendChange(new ModifyRoadSegment(
                    recordNumber,
                    roadSegmentId,
                    roadSegment.AttributeHash.GeometryDrawMethod,
                    geometry: geometry
                )
                .WithLane(lane)
                .WithSurface(surface)
                .WithWidth(width));
        }, cancellationToken);

        var lastHash = await GetRoadSegmentHash(roadSegmentId, RoadSegmentGeometryDrawMethod.Outlined, cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(ChangeRoadSegmentOutlineGeometrySqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
