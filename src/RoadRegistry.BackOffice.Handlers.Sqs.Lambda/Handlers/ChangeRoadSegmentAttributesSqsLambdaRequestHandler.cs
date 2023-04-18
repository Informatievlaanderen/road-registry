namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
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

            foreach (var change in request.Request.ChangeRequests)
            {
                var roadSegmentId = new RoadSegmentId(change.Id);
                var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);

                var geometryDrawMethod = roadSegment?.AttributeHash.GeometryDrawMethod;

                translatedChanges = translatedChanges.AppendChange(new ModifyRoadSegmentAttributes(recordNumber, roadSegmentId, geometryDrawMethod)
                {
                    MaintenanceAuthority = change.MaintenanceAuthority is not null ? new OrganizationId(change.MaintenanceAuthority) : null,
                    Morphology = change.Morphology is not null ? RoadSegmentMorphology.Parse(change.Morphology) : null,
                    Status = change.Status is not null ? RoadSegmentStatus.Parse(change.Status) : null,
                    Category = change.Category is not null ? RoadSegmentCategory.Parse(change.Category) : null,
                    AccessRestriction = change.AccessRestriction is not null ? RoadSegmentAccessRestriction.Parse(change.AccessRestriction) : null
                });

                recordNumber = recordNumber.Next();
            }

            return translatedChanges;
        }, cancellationToken);

        return new ETagResponse(string.Empty, string.Empty);
    }
}
