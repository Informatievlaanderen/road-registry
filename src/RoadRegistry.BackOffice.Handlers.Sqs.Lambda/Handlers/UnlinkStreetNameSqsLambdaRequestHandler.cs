namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Extensions;
using FeatureToggles;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure.Extensions;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class UnlinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<UnlinkStreetNameSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly DistributedStreamStoreLockOptions _distributedStreamStoreLockOptions;

    public UnlinkStreetNameSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<UnlinkStreetNameSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _distributedStreamStoreLockOptions = distributedStreamStoreLockOptions;
    }

    protected override async Task<object> InnerHandle(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var roadSegmentId = new RoadSegmentId(request.Request.WegsegmentId);
        var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(request.Request.Methode);
        var streamName = RoadNetworkStreamNameProvider.Get(roadSegmentId, geometryDrawMethod);
        var distributedStreamStoreLock = new DistributedStreamStoreLock(_distributedStreamStoreLockOptions, streamName, Logger);

        await distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Straatnaam ontkoppelen", async translatedChanges =>
            {
                var problems = Problems.None;

                var roadSegment = await RoadRegistryContext.RoadNetworks.FindRoadSegment(roadSegmentId, geometryDrawMethod, cancellationToken);
                if (roadSegment == null)
                {
                    problems += new RoadSegmentNotFound(roadSegmentId);
                    throw new RoadRegistryProblemsException(problems);
                }

                var recordNumber = RecordNumber.Initial;
                var attributeId = AttributeId.Initial;

                var leftStreetNameId = request.Request.LinkerstraatnaamId.GetIdentifierFromPuri();
                var rightStreetNameId = request.Request.RechterstraatnaamId.GetIdentifierFromPuri();

                if (leftStreetNameId > 0 || rightStreetNameId > 0)
                {
                    if (leftStreetNameId > 0)
                    {
                        if (StreetNameLocalId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId) || (roadSegment.AttributeHash.LeftStreetNameId ?? 0) != leftStreetNameId)
                        {
                            problems += new RoadSegmentStreetNameLeftNotLinked(request.Request.WegsegmentId, request.Request.LinkerstraatnaamId);
                        }
                    }

                    if (rightStreetNameId > 0)
                    {
                        if (StreetNameLocalId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId) || (roadSegment.AttributeHash.RightStreetNameId ?? 0) != rightStreetNameId)
                        {
                            problems += new RoadSegmentStreetNameRightNotLinked(request.Request.WegsegmentId, request.Request.RechterstraatnaamId);
                        }
                    }

                    var modifyRoadSegment = new ModifyRoadSegment(
                        recordNumber,
                        roadSegment.Id,
                        roadSegment.Start,
                        roadSegment.End,
                        roadSegment.AttributeHash.OrganizationId,
                        roadSegment.AttributeHash.GeometryDrawMethod,
                        roadSegment.AttributeHash.Morphology,
                        roadSegment.AttributeHash.Status,
                        roadSegment.AttributeHash.Category,
                        roadSegment.AttributeHash.AccessRestriction,
                        leftStreetNameId > 0 ? StreetNameLocalId.NotApplicable : roadSegment.AttributeHash.LeftStreetNameId,
                        rightStreetNameId > 0 ? StreetNameLocalId.NotApplicable : roadSegment.AttributeHash.RightStreetNameId
                    ).WithGeometry(roadSegment.Geometry);

                    foreach (var lane in roadSegment.Lanes)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithLane(new RoadSegmentLaneAttribute(attributeId, lane.Count, lane.Direction, lane.From, lane.To));
                        attributeId = attributeId.Next();
                    }
                    foreach (var surface in roadSegment.Surfaces)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithSurface(new RoadSegmentSurfaceAttribute(attributeId, surface.Type, surface.From, surface.To));
                        attributeId = attributeId.Next();
                    }
                    foreach (var width in roadSegment.Widths)
                    {
                        modifyRoadSegment = modifyRoadSegment.WithWidth(new RoadSegmentWidthAttribute(attributeId, width.Width, width.From, width.To));
                        attributeId = attributeId.Next();
                    }

                    translatedChanges = translatedChanges.AppendChange(modifyRoadSegment);
                }

                if (problems.Any())
                {
                    throw new RoadRegistryProblemsException(problems);
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        var lastHash = await GetRoadSegmentHash(roadSegmentId, geometryDrawMethod, cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
