namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Core;
using Editor.Schema;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingService.Abstractions;
using RemoveRoadSegments = Messages.RemoveRoadSegments;

public sealed class DeleteRoadSegmentsRequestHandler : EndpointRequestHandler<DeleteRoadSegmentsRequest, DeleteRoadSegmentsResponse>
{
    private readonly EditorContext _editorContext;
    private readonly ITicketing _ticketing;
    private readonly ITicketingUrl _ticketingUrl;

    public DeleteRoadSegmentsRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        EditorContext editorContext,
        ITicketing ticketing,
        ITicketingUrl ticketingUrl,
        ILoggerFactory loggerFactory)
        : base(roadNetworkCommandQueue, loggerFactory.CreateLogger<DeleteRoadSegmentsRequestHandler>())
    {
        _editorContext = editorContext;
        _ticketing = ticketing;
        _ticketingUrl = ticketingUrl;
    }

    protected override async Task<DeleteRoadSegmentsResponse> InnerHandleAsync(DeleteRoadSegmentsRequest request, CancellationToken cancellationToken)
    {
        var ids = request.RoadSegmentIds.Select(x => x.ToInt32()).ToArray();

        var roadSegments = await _editorContext.RoadSegments
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(cancellationToken);

        var nonExistingIds = request.RoadSegmentIds.Except(roadSegments.Select(x => new RoadSegmentId(x.Id))).ToArray();
        if (nonExistingIds.Any())
        {
            throw new RoadSegmentsNotFoundException(nonExistingIds);
        }

        var requestedChanges = roadSegments
            .GroupBy(x => x.MethodId)
            .Select(x => new RequestedChange
            {
                RemoveRoadSegments = new RemoveRoadSegments
                {
                    GeometryDrawMethod = RoadSegmentGeometryDrawMethod.ByIdentifier[x.Key],
                    Ids = x.Select(rs => rs.Id).ToArray()
                }
            })
            .ToArray();

        var changeRoadNetwork = new ChangeRoadNetwork
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(Guid.NewGuid())),
            Changes = requestedChanges,
            Reason = "Verwijder wegsegmenten in bulk",
            Operator = request.ProvenanceData.Operator,
            OrganizationId = OrganizationId.DigitaalVlaanderen
        };
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, cancellationToken);

        var ticketId = await _ticketing.CreateTicket(new Dictionary<string, string>
        {
            { "Registry", nameof(RoadRegistry) },
            { "Action", "DeleteRoadSegments" }
        }, cancellationToken);
        changeRoadNetwork.TicketId = ticketId;

        await Queue(new Command(changeRoadNetwork), cancellationToken);

        return new DeleteRoadSegmentsResponse(_ticketingUrl.For(ticketId));
    }
}
