namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Extensions;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public sealed class UnlinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<UnlinkStreetNameSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;

    public UnlinkStreetNameSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkCommandQueue commandQueue,
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
        _commandQueue = commandQueue;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
    }

    protected override async Task<ETagResponse> InnerHandleAsync(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _commandQueue.DispatchChangeRoadNetwork(IdempotentCommandHandler, request, "Straatnaam ontkoppelen", async translatedChanges =>
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

                if (leftStreetNameId > 0)
                {
                    if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId) || (roadSegment.AttributeHash.LeftStreetNameId ?? 0) != leftStreetNameId)
                    {
                        throw new RoadRegistryValidationException(
                            ValidationErrors.RoadSegment.StreetName.Left.NotLinked.Message(request.Request.WegsegmentId, request.Request.LinkerstraatnaamId!),
                            ValidationErrors.RoadSegment.StreetName.Left.NotLinked.Code);
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
                        null,
                        roadSegment.AttributeHash.RightStreetNameId
                    ).WithGeometry(roadSegment.Geometry));
                }
                else if (rightStreetNameId > 0)
                {
                    if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId) || (roadSegment.AttributeHash.RightStreetNameId ?? 0) != rightStreetNameId)
                    {
                        throw new RoadRegistryValidationException(
                            ValidationErrors.RoadSegment.StreetName.Right.NotLinked.Message(request.Request.WegsegmentId, request.Request.RechterstraatnaamId!),
                            ValidationErrors.RoadSegment.StreetName.Right.NotLinked.Code);
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
                        roadSegment.AttributeHash.LeftStreetNameId,
                        null
                    ).WithGeometry(roadSegment.Geometry));
                }
                else
                {
                    throw new RoadRegistryValidationException(
                        ValidationErrors.Common.IncorrectObjectId.Message(request.Request.LinkerstraatnaamId),
                        ValidationErrors.Common.IncorrectObjectId.Code);
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
