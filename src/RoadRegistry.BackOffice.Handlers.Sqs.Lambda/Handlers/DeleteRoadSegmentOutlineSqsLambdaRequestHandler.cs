namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadSegment.ValueObjects;
using TicketingService.Abstractions;
using RemoveRoadSegment = BackOffice.Uploads.RemoveRoadSegment;

public sealed class DeleteRoadSegmentOutlineSqsLambdaRequestHandler : SqsLambdaHandler<DeleteRoadSegmentOutlineSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public DeleteRoadSegmentOutlineSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        ILogger<DeleteRoadSegmentOutlineSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
    }

    protected override async Task<object> InnerHandle(DeleteRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var roadSegmentId = new RoadSegmentId(request.Request.WegsegmentId);

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Verwijder ingeschetst wegsegment", async translatedChanges =>
        {
            var network = await RoadRegistryContext.RoadNetworks.ForOutlinedRoadSegment(roadSegmentId, cancellationToken);

            var roadSegment = network.FindRoadSegment(roadSegmentId);
            if (roadSegment == null || roadSegment.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            {
                throw new RoadSegmentOutlinedNotFoundException();
            }

            var recordNumber = RecordNumber.Initial;

            return translatedChanges.AppendChange(new RemoveRoadSegment(
                recordNumber,
                roadSegment.Id,
                roadSegment.AttributeHash.GeometryDrawMethod
            ));
        }, cancellationToken);

        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), string.Empty);
    }

    protected override Task ValidateIfMatchHeaderValue(DeleteRoadSegmentOutlineSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
