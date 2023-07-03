namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

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

public sealed class UnlinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<UnlinkStreetNameSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;

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
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override async Task<object> InnerHandle(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Straatnaam ontkoppelen", async translatedChanges =>
            {
                var problems = Problems.None;

                var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
                var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(request.Request.WegsegmentId));
                if (roadSegment == null)
                {
                    problems = problems.Add(new RoadSegmentNotFound());
                    throw new RoadRegistryProblemsException(problems);
                }

                var recordNumber = RecordNumber.Initial;

                var leftStreetNameId = request.Request.LinkerstraatnaamId.GetIdentifierFromPuri();
                var rightStreetNameId = request.Request.RechterstraatnaamId.GetIdentifierFromPuri();

                if (leftStreetNameId > 0 || rightStreetNameId > 0)
                {
                    if (leftStreetNameId > 0)
                    {
                        if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId) || (roadSegment.AttributeHash.LeftStreetNameId ?? 0) != leftStreetNameId)
                        {
                            problems = problems.Add(new RoadSegmentStreetNameLeftNotLinked(request.Request.WegsegmentId, request.Request.LinkerstraatnaamId));
                        }
                    }

                    if (rightStreetNameId > 0)
                    {
                        if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId) || (roadSegment.AttributeHash.RightStreetNameId ?? 0) != rightStreetNameId)
                        {
                            problems = problems.Add(new RoadSegmentStreetNameRightNotLinked(request.Request.WegsegmentId, request.Request.RechterstraatnaamId));
                        }
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
                        leftStreetNameId > 0 ? CrabStreetnameId.NotApplicable : roadSegment.AttributeHash.LeftStreetNameId,
                        rightStreetNameId > 0 ? CrabStreetnameId.NotApplicable : roadSegment.AttributeHash.RightStreetNameId
                    ).WithGeometry(roadSegment.Geometry));
                }

                if (problems.Any())
                {
                    throw new RoadRegistryProblemsException(problems);
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        var roadSegmentId = request.Request.WegsegmentId;
        var lastHash = await GetRoadSegmentHash(new RoadSegmentId(roadSegmentId), cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
