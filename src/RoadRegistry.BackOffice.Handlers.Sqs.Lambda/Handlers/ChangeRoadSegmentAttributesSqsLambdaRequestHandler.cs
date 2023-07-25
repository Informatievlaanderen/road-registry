namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Core;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using TicketingService.Abstractions;
using ModifyRoadSegmentAttributes = BackOffice.Uploads.ModifyRoadSegmentAttributes;

public sealed class ChangeRoadSegmentAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentAttributesSqsLambdaRequest>
{
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;

    public ChangeRoadSegmentAttributesSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        ILogger<ChangeRoadSegmentAttributesSqsLambdaRequestHandler> logger)
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

    protected override async Task<object> InnerHandle(ChangeRoadSegmentAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        // Do NOT lock the stream store for stream RoadNetworks.Stream

        await _changeRoadNetworkDispatcher.DispatchAsync(request, "Attributen wijzigen", async translatedChanges =>
        {
            var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

            var recordNumber = RecordNumber.Initial;
            var problems = Problems.None;

            foreach (var change in request.Request.ChangeRequests)
            {
                var roadSegmentId = new RoadSegmentId(change.Id);
                var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
                if (roadSegment is null)
                {
                    problems = problems.Add(new RoadSegmentNotFound(roadSegmentId));
                    continue;
                }

                var geometryDrawMethod = roadSegment.AttributeHash.GeometryDrawMethod;
                
                translatedChanges = translatedChanges.AppendChange(new ModifyRoadSegmentAttributes(recordNumber, roadSegmentId, geometryDrawMethod)
                {
                    MaintenanceAuthority = change.MaintenanceAuthority,
                    Morphology = change.Morphology,
                    Status = change.Status,
                    Category = change.Category,
                    AccessRestriction = change.AccessRestriction
                });

                recordNumber = recordNumber.Next();
            }

            if (problems.Any())
            {
                throw new RoadRegistryProblemsException(problems);
            }

            return translatedChanges;
        }, cancellationToken);

        return new ChangeRoadSegmentAttributesResponse();
    }
}
