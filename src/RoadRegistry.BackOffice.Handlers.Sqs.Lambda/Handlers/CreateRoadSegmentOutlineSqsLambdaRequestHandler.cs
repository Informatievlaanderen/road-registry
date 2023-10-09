namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
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
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;

    public CreateRoadSegmentOutlineSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
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
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override async Task<object> InnerHandle(CreateRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            var changeRoadNetworkCommand = await _changeRoadNetworkDispatcher.DispatchAsync(request, "Wegsegment schetsen", async translatedChanges =>
            {
                var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
                var roadSegmentId = network.ProvidesNextRoadSegmentId()();

                var r = request.Request;
                var recordNumber = RecordNumber.Initial;

                var geometry = GeometryTranslator.Translate(r.Geometry);

                var fromPosition = new RoadSegmentPosition(0);
                var toPosition = new RoadSegmentPosition((decimal)geometry.Length);
                //TODO-rik #797 use orgreader to find OrgId
                translatedChanges = translatedChanges.AppendChange(
                    new AddRoadSegment(
                            recordNumber,
                            roadSegmentId,
                            roadSegmentId,
                            r.MaintenanceAuthority,
                            RoadSegmentGeometryDrawMethod.Outlined,
                            r.Morphology,
                            r.Status,
                            RoadSegmentCategory.Unknown,
                            r.AccessRestriction)
                        .WithGeometry(geometry)
                        .WithSurface(new RoadSegmentSurfaceAttribute(network.ProvidesNextRoadSegmentSurfaceAttributeId()(roadSegmentId)(), r.SurfaceType, fromPosition, toPosition))
                        .WithWidth(new RoadSegmentWidthAttribute(network.ProvidesNextRoadSegmentWidthAttributeId()(roadSegmentId)(), r.Width, fromPosition, toPosition))
                        .WithLane(new RoadSegmentLaneAttribute(network.ProvidesNextRoadSegmentLaneAttributeId()(roadSegmentId)(), r.LaneCount, r.LaneDirection, fromPosition, toPosition))
                );

                return translatedChanges;
            }, cancellationToken);

            var roadSegmentId = new RoadSegmentId(changeRoadNetworkCommand.Changes.Single().AddRoadSegment.TemporaryId);
            Logger.LogInformation("Created road segment {RoadSegmentId}", roadSegmentId);
            var lastHash = await GetRoadSegmentHash(roadSegmentId, cancellationToken);
            return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
        }, cancellationToken);
    }
}
