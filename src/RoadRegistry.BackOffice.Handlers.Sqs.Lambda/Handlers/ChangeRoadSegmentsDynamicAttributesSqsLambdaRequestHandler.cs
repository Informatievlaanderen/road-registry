namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.RoadSegments;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Editor.Schema;
using Exceptions;
using Hosts;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using TicketingService.Abstractions;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly EditorContext _editorContext;
    private readonly VerificationContextTolerances _tolerances = VerificationContextTolerances.Default;

    public ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
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
        _editorContext = editorContext;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Dynamische attributen wijzigen", async translatedChanges =>
            {
                var recordNumber = RecordNumber.Initial;
                var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

                var problems = Problems.None;

                foreach (var change in request.Request.ChangeRequests)
                {
                    var roadSegmentId = new RoadSegmentId(change.Id);

                    var editorRoadSegment = await _editorContext.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == change.Id, cancellationToken);
                    if (editorRoadSegment is null)
                    {
                        problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                        continue;
                    }

                    var geometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[editorRoadSegment.MethodId];

                    var networkRoadSegment = await RoadRegistryContext.RoadNetworks.FindRoadSegment(roadSegmentId, geometryDrawMethod, cancellationToken);
                    if (networkRoadSegment is null)
                    {
                        problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                        continue;
                    }

                    var modifyChange = new ModifyRoadSegmentAttributes(recordNumber, roadSegmentId, geometryDrawMethod)
                    {
                        Lanes = change.Lanes?
                            .Select(lane => new RoadSegmentLaneAttribute(attributeIdProvider.Next(), lane.Count, lane.Direction, lane.FromPosition, lane.ToPosition))
                            .ToArray(),
                        Surfaces = change.Surfaces?
                            .Select(surface => new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), surface.Type, surface.FromPosition, surface.ToPosition))
                            .ToArray(),
                        Widths = change.Widths?
                            .Select(width => new RoadSegmentWidthAttribute(attributeIdProvider.Next(), width.Width, width.FromPosition, width.ToPosition))
                            .ToArray()
                    };

                    problems += GetProblemsForLanes(networkRoadSegment, modifyChange.Lanes);
                    problems += GetProblemsForSurfaces(networkRoadSegment, modifyChange.Surfaces);
                    problems += GetProblemsForWidths(networkRoadSegment, modifyChange.Widths);

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

    private Problems GetProblemsForLanes(RoadSegment roadSegment, RoadSegmentLaneAttribute[]? lanes)
    {
        var problems = Problems.None;

        if (lanes is null)
        {
            return problems;
        }

        var last = lanes.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, _tolerances))
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

    private Problems GetProblemsForSurfaces(RoadSegment roadSegment, RoadSegmentSurfaceAttribute[]? surfaces)
    {
        var problems = Problems.None;

        if (surfaces is null)
        {
            return problems;
        }

        var last = surfaces.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, _tolerances))
        {
            problems += new RoadSegmentSurfaceAttributeToPositionNotEqualToLength(last.TemporaryId, last.To, roadSegment.Geometry.Length);
        }

        return problems;
    }

    private Problems GetProblemsForWidths(RoadSegment roadSegment, RoadSegmentWidthAttribute[]? widths)
    {
        var problems = Problems.None;

        if (widths is null)
        {
            return problems;
        }

        var last = widths.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, _tolerances))
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
