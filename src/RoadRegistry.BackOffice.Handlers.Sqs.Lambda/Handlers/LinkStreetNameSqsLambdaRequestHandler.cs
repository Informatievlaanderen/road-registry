namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions;
using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Extensions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public sealed class LinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<LinkStreetNameSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly IStreetNameCache _streetNameCache;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public LinkStreetNameSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IStreetNameCache streetNameCache,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<LinkStreetNameSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _streetNameCache = streetNameCache;
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override async Task<object> InnerHandle(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Straatnaam koppelen", async translatedChanges =>
            {
                var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
                var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(request.Request.WegsegmentId));
                if (roadSegment == null)
                {
                    throw new RoadSegmentNotFoundException();
                }

                var recordNumber = RecordNumber.Initial;

                var leftStreetNameId = request.Request.LinkerstraatnaamId.GetIdentifierFromPuri();
                var rightStreetNameId = request.Request.RechterstraatnaamId.GetIdentifierFromPuri();

                if (leftStreetNameId > 0 || rightStreetNameId > 0)
                {
                    if (leftStreetNameId > 0)
                    {
                        if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId))
                        {
                            throw new RoadRegistryValidationException(new RoadSegmentStreetNameLeftNotUnlinked(request.Request.WegsegmentId));
                        }

                        await ValidateStreetName(leftStreetNameId, cancellationToken);
                    }

                    if (rightStreetNameId > 0)
                    {
                        if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId))
                        {
                            throw new RoadRegistryValidationException(new RoadSegmentStreetNameRightNotUnlinked(request.Request.WegsegmentId));
                        }

                        await ValidateStreetName(rightStreetNameId, cancellationToken);
                    }

                    translatedChanges = translatedChanges.AppendChange(new ModifyRoadSegment(
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
                        leftStreetNameId > 0 ? new CrabStreetnameId(leftStreetNameId) : roadSegment.AttributeHash.LeftStreetNameId,
                        rightStreetNameId > 0 ? new CrabStreetnameId(rightStreetNameId) : roadSegment.AttributeHash.RightStreetNameId
                    ).WithGeometry(roadSegment.Geometry));
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        var roadSegmentId = request.Request.WegsegmentId;
        var lastHash = await GetRoadSegmentHash(new RoadSegmentId(roadSegmentId), cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ValidateStreetName(int streetNameId, CancellationToken cancellationToken)
    {
        var streetNameStatuses = await _streetNameCache.GetStreetNameStatusesById(new[] { streetNameId }, cancellationToken);
        if (!streetNameStatuses.TryGetValue(streetNameId, out var streetNameStatus))
        {
            throw new RoadRegistryValidationException(new StreetNameNotFound());
        }
        if (streetNameStatus is null)
        {
            throw new RoadRegistryValidationException(new StreetNameNotFound());
        }

        if (!string.Equals(streetNameStatus, "proposed", StringComparison.InvariantCultureIgnoreCase)
            && !string.Equals(streetNameStatus, "current", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new RoadRegistryValidationException(new RoadSegmentStreetNameNotProposedOrCurrent());
        }
    }
}
