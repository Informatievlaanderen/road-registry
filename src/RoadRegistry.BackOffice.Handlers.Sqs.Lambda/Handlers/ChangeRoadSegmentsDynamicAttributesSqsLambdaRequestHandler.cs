namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using TicketingService.Abstractions;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;

public sealed class ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler> logger)
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

    protected override async Task<object> InnerHandle(ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
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
                        problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
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

                    problems += GetProblemsForLanes(roadSegment, modifyChange.Lanes);
                    problems += GetProblemsForSurfaces(roadSegment, modifyChange.Surfaces);
                    problems += GetProblemsForWidths(roadSegment, modifyChange.Widths);
                    
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

        return new ChangeRoadSegmentsDynamicAttributesResponse();
    }

    private Problems GetProblemsForLanes(RoadSegment roadSegment, BackOffice.Uploads.RoadSegmentLaneAttribute[]? lanes)
    {
        var problems = Problems.None;

        if (lanes is null)
        {
            return problems;
        }

        var last = lanes.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.MeasurementTolerance))
        {
            problems += new RoadSegmentLaneAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length);
        }

        if (roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            foreach (var lane in lanes)
            {
                if (!lane.Count.IsValidForEdit())
                {
                    problems += new RoadSegmentLaneCountNotValid(lane.Count);
                }
            }
        }

        return problems;
    }

    private Problems GetProblemsForSurfaces(RoadSegment roadSegment, BackOffice.Uploads.RoadSegmentSurfaceAttribute[]? surfaces)
    {
        var problems = Problems.None;

        if (surfaces is null)
        {
            return problems;
        }

        var last = surfaces.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.MeasurementTolerance))
        {
            problems += new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length);
        }

        return problems;
    }

    private Problems GetProblemsForWidths(RoadSegment roadSegment, BackOffice.Uploads.RoadSegmentWidthAttribute[]? widths)
    {
        var problems = Problems.None;

        if (widths is null)
        {
            return problems;
        }

        var last = widths.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.MeasurementTolerance))
        {
            problems += new RoadSegmentWidthAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length);
        }

        if (roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
        {
            foreach (var width in widths)
            {
                if (!width.Width.IsValidForEdit())
                {
                    problems += new RoadSegmentWidthNotValid(width.Width);
                }
            }
        }

        return problems;
    }
}
