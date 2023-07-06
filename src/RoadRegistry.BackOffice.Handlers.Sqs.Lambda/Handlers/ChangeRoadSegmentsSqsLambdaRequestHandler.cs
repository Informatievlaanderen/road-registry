namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.BasicApiProblem;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using FluentValidation;
using FluentValidation.Results;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Exceptions;
using TicketingService.Abstractions;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;

public sealed class ChangeRoadSegmentsSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentsSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public ChangeRoadSegmentsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<ChangeRoadSegmentsSqsLambdaRequestHandler> logger)
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

    protected override async Task<object> InnerHandle(ChangeRoadSegmentsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Wegsegmenten wijzigen", async translatedChanges =>
            {
                var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

                var recordNumber = RecordNumber.Initial;
                var problems = Problems.None;

                foreach (var change in request.Request.ChangeRequests)
                {
                    var roadSegmentId = new RoadSegmentId(change.Id);
                    var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
                    if (roadSegment is null)
                    {
                        problems = problems.Add(new RoadSegmentNotFound());
                        continue;
                    }

                    var geometryDrawMethod = roadSegment.AttributeHash.GeometryDrawMethod;

                    var laneAttributeId = roadNetwork.ProvidesNextRoadSegmentLaneAttributeId()(roadSegmentId);
                    var surfaceAttributeId = roadNetwork.ProvidesNextRoadSegmentSurfaceAttributeId()(roadSegmentId);
                    var widthAttributeId = roadNetwork.ProvidesNextRoadSegmentWidthAttributeId()(roadSegmentId);

                    var modifyChange = new ModifyRoadSegmentAttributes(recordNumber, roadSegmentId, geometryDrawMethod)
                    {
                        Lanes = change.Lanes?
                            .Select(lane => new BackOffice.Uploads.RoadSegmentLaneAttribute(laneAttributeId(), lane.Count, lane.Direction, lane.FromPosition, lane.ToPosition))
                            .ToArray(),
                        Surfaces = change.Surfaces?
                            .Select(surface => new BackOffice.Uploads.RoadSegmentSurfaceAttribute(surfaceAttributeId(), surface.Type, surface.FromPosition, surface.ToPosition))
                            .ToArray(),
                        Widths = change.Widths?
                            .Select(width => new BackOffice.Uploads.RoadSegmentWidthAttribute(widthAttributeId(), width.Width, width.FromPosition, width.ToPosition))
                            .ToArray()
                    };
                    
                    if (modifyChange.Lanes is not null)
                    {
                        var last = modifyChange.Lanes.Last();
                        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.DynamicRoadSegmentAttributePositionTolerance))
                        {
                            problems = problems.Add(new RoadSegmentLaneAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length));
                        }
                    }

                    if (modifyChange.Surfaces is not null)
                    {
                        var last = modifyChange.Surfaces.Last();
                        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.DynamicRoadSegmentAttributePositionTolerance))
                        {
                            problems = problems.Add(new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length));
                        }
                    }

                    if (modifyChange.Widths is not null)
                    {
                        var last = modifyChange.Widths.Last();
                        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.DynamicRoadSegmentAttributePositionTolerance))
                        {
                            problems = problems.Add(new RoadSegmentWidthAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length));
                        }
                    }

                    translatedChanges = translatedChanges.AppendChange(modifyChange);

                    recordNumber = recordNumber.Next();
                }

                if (problems.Any())
                {
                    throw new RoadRegistryProblemsException(problems);
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        return new ChangeRoadSegmentsResponse();
    }
}
