namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentOutlineGeometrySqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        ILogger<ChangeRoadSegmentOutlineGeometrySqsLambdaRequestHandler> logger)
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
    
    protected override async Task<ETagResponse> InnerHandleAsync(ChangeRoadSegmentOutlineGeometrySqsLambdaRequest request, CancellationToken cancellationToken)
    {
        // Do NOT lock the stream store for stream RoadNetworks.Stream

        var roadSegmentId = request.Request.RoadSegmentId;

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Wijzig geometrie van ingeschetst wegsegment", async translatedChanges =>
        {
            var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

            var roadSegment = network.FindRoadSegment(roadSegmentId);
            if (roadSegment == null || roadSegment.AttributeHash.GeometryDrawMethod != RoadSegmentGeometryDrawMethod.Outlined)
            {
                throw new RoadSegmentNotFoundException();
            }

            var recordNumber = RecordNumber.Initial;

            return translatedChanges.AppendChange(new BackOffice.Uploads.ModifyRoadSegmentAttributes(
                recordNumber,
                roadSegmentId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                geometry: request.Request.Geometry
            ));
        }, cancellationToken);

        var lastHash = await GetRoadSegmentHash(new RoadSegmentId(roadSegmentId), cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }

    protected override Task ValidateIfMatchHeaderValue(ChangeRoadSegmentOutlineGeometrySqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
