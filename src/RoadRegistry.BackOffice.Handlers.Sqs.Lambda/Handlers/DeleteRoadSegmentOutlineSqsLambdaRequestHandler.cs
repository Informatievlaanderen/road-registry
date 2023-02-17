namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Hosts.Infrastructure.Extensions;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.BackOffice.Core;
using TicketingService.Abstractions;
using RemoveRoadSegment = BackOffice.Uploads.RemoveRoadSegment;

public sealed class DeleteRoadSegmentOutlineSqsLambdaRequestHandler : SqsLambdaHandler<DeleteRoadSegmentOutlineSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;

    public DeleteRoadSegmentOutlineSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkCommandQueue commandQueue,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<DeleteRoadSegmentOutlineSqsLambdaRequestHandler> logger)
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

    protected override Task<ETagResponse> InnerHandleAsync(DeleteRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            var roadSegmentId = new RoadSegmentId(request.Request.WegsegmentId);

            await _commandQueue.DispatchChangeRoadNetwork(IdempotentCommandHandler, request, "Verwijder ingeschetst wegsegment", async translatedChanges =>
            {
                var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

                var roadSegment = network.FindRoadSegment(roadSegmentId);
                if (roadSegment == null || roadSegment.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
                {
                    throw new RoadSegmentNotFoundException();
                }

                var recordNumber = RecordNumber.Initial;

                return translatedChanges.AppendChange(new RemoveRoadSegment(
                    recordNumber,
                    roadSegment.Id
                ));
            }, cancellationToken);

            return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), string.Empty);
        }, cancellationToken);
    }

    protected override Task ValidateIfMatchHeaderValue(DeleteRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
