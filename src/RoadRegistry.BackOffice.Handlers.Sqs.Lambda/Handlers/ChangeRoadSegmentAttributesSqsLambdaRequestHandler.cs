namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;

public sealed class ChangeRoadSegmentAttributesSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadSegmentAttributesSqsLambdaRequest>
{
    private readonly IRoadNetworkEventWriter _eventWriter;

    public ChangeRoadSegmentAttributesSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkEventWriter eventWriter,
        ILogger<ChangeRoadSegmentAttributesSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            null,
            roadRegistryContext,
            logger)
    {
        _eventWriter = eventWriter;
    }

    protected override async Task<ETagResponse> InnerHandleAsync(ChangeRoadSegmentAttributesSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var changes = request.Request.ChangeRequests;
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

        var roadSegmentIds = changes.Select(s => s.Id).Distinct();
        var roadSegments = roadNetwork.FindRoadSegments(roadSegmentIds);

        foreach (var roadSegment in roadSegments)
        {
            var attributeChanges = changes.Where(w => w.Id.Equals(roadSegment.Id));

            foreach (var attributeChange in attributeChanges)
            {
                switch (attributeChange)
                {
                    case ChangeRoadSegmentMaintenanceAuthorityAttributeRequest maintenanceAuthority:
                        roadSegment.WithMaintenanceAuthorityAttribute(maintenanceAuthority.MaintenanceAuthority);
                        break;

                    case ChangeRoadSegmentStatusAttributeRequest status:
                        roadSegment.WithStatusAttribute(status.Status);
                        break;

                    case ChangeRoadSegmentMorphologyAttributeRequest morphology:
                        roadSegment.WithMorphologyAttribute(morphology.Morphology);
                        break;

                    case ChangeRoadSegmentAccessRestrictionAttributeRequest accessRestriction:
                        roadSegment.WithAccessRestrictionAttribute(accessRestriction.AccessRestriction);
                        break;

                    case ChangeRoadSegmentCategoryAttributeRequest category:
                        roadSegment.WithCategoryAttribute(category.Category);
                        break;
                }
            }
        }

        //var roadSegmentId = request.Request;
        //var lastHash = await GetRoadSegmentHash(new RoadSegmentId(roadSegmentId), cancellationToken);
        return new ETagResponse(string.Empty, string.Empty);
    }
}
