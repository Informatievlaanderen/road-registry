namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Hosts.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Requests;
using RoadSegments;

public sealed class SqsLambdaRequestHandler : SqsMessageRequestHandler
{
    public SqsLambdaRequestHandler(IMediator mediator, ILogger<SqsMessageRequestHandler> logger) : base(mediator, logger)
    {
    }

    public override SqsLambdaRequest ConvertSqsLambdaRequest(SqsRequest sqsRequest, string groupId) => sqsRequest switch
    {
        LinkStreetNameSqsRequest linkStreetNameSqsRequest => new LinkStreetNameSqsLambdaRequest(groupId, linkStreetNameSqsRequest),
        UnlinkStreetNameSqsRequest unlinkStreetNameSqsRequest => new UnlinkStreetNameSqsLambdaRequest(groupId, unlinkStreetNameSqsRequest),
        CorrectRoadSegmentVersionsSqsRequest correctRoadSegmentVersionsSqsRequest => new CorrectRoadSegmentVersionsSqsLambdaRequest(groupId, correctRoadSegmentVersionsSqsRequest),
        CreateRoadSegmentOutlineSqsRequest createRoadSegmentOutlineSqsRequest => new CreateRoadSegmentOutlineSqsLambdaRequest(groupId, createRoadSegmentOutlineSqsRequest),
        _ => throw new NotImplementedException(
            $"{sqsRequest.GetType().Name} has no corresponding {nameof(SqsLambdaRequest)} defined.")
    };
}
