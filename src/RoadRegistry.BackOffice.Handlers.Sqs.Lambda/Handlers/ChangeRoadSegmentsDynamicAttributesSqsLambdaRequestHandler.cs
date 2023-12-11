namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

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
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Extensions;
using TicketingService.Abstractions;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;

public sealed class ChangeRoadSegmentsDynamicAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly EditorContext _editorContext;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;

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
        _manager = manager;
        _fileEncoding = fileEncoding;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override async Task<object> InnerHandle(ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Dynamische attributen wijzigen", async translatedChanges =>
            {
                var recordNumber = RecordNumber.Initial;
                var attributeId = AttributeId.Initial;
                AttributeId GetNextAttributeId()
                {
                    attributeId = attributeId.Next();
                    return attributeId;
                }

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

                    var modifyChange = new ModifyRoadSegmentAttributes(recordNumber, roadSegmentId, geometryDrawMethod)
                    {
                        Lanes = change.Lanes?
                            .Select(lane => new BackOffice.Uploads.RoadSegmentLaneAttribute(GetNextAttributeId(), lane.Count, lane.Direction, lane.FromPosition, lane.ToPosition))
                            .ToArray(),
                        Surfaces = change.Surfaces?
                            .Select(surface => new BackOffice.Uploads.RoadSegmentSurfaceAttribute(GetNextAttributeId(), surface.Type, surface.FromPosition, surface.ToPosition))
                            .ToArray(),
                        Widths = change.Widths?
                            .Select(width => new BackOffice.Uploads.RoadSegmentWidthAttribute(GetNextAttributeId(), width.Width, width.FromPosition, width.ToPosition))
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

    private Problems GetProblemsForLanes(RoadSegment roadSegment, BackOffice.Uploads.RoadSegmentLaneAttribute[]? lanes)
    {
        var problems = Problems.None;

        if (lanes is null)
        {
            return problems;
        }

        var last = lanes.Last();
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.GeometryTolerance))
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
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.GeometryTolerance))
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
        if (!last.To.ToDouble().IsReasonablyEqualTo(roadSegment.Geometry.Length, DefaultTolerances.GeometryTolerance))
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
