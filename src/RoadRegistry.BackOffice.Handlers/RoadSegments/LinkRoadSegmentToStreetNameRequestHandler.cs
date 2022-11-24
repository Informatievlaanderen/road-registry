namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using FluentValidation;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public class LinkRoadSegmentToStreetNameRequestHandler : EndpointRequestHandler<LinkRoadSegmentToStreetNameRequest, LinkRoadSegmentToStreetNameResponse>
{
    private readonly IStreamStore _store;
    private readonly IRoadRegistryContext _roadRegistryContext;

    public LinkRoadSegmentToStreetNameRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<LinkRoadSegmentToStreetNameRequestHandler> logger,
        IStreamStore store,
        IRoadRegistryContext roadRegistryContext)
        : base(dispatcher, logger)
    {
        _store = store;
        _roadRegistryContext = roadRegistryContext;
    }

    public override async Task<LinkRoadSegmentToStreetNameResponse> HandleAsync(LinkRoadSegmentToStreetNameRequest request, CancellationToken cancellationToken)
    {
        var messageId = Guid.NewGuid(); //TODO-rik van waar halen?
        
        var requestId = ChangeRequestId.FromUploadId(new UploadId(messageId));
        var translatedChanges = TranslatedChanges.Empty;

        var roadNetwork = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(request.RoadSegmentId));
        if (roadSegment != null)
        {
            var recordNumber = new RecordNumber(1); //TODO-rik moet dit iets anders zijn?

            if (request.LeftStreetNameId > 0)
            {
                if (roadSegment.AttributeHash.LeftStreetNameId > 0)
                {
                    throw new ValidationException("RoadSegment LeftStreetNameId must be empty");
                }

                translatedChanges.AppendChange(new ModifyRoadSegment(
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
                    new CrabStreetnameId(request.LeftStreetNameId),
                    roadSegment.AttributeHash.RightStreetNameId
                ));
            }
            else if (request.RightStreetNameId > 0)
            {
                if (roadSegment.AttributeHash.RightStreetNameId != null)
                {
                    throw new ValidationException("RoadSegment LeftStreetNameId must be empty");
                }

                translatedChanges.AppendChange(new ModifyRoadSegment(
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
                    new CrabStreetnameId(request.RightStreetNameId)
                ));
            }
            else
            {
                throw new ValidationException("LeftStreetNameId or RightStreetNameId is required");
            }
        }
        else
        {
            throw new ValidationException("RoadSegment not found");
        }

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();

        var command = new Command(new ChangeRoadNetwork
            {
                RequestId = requestId,
                Changes = requestedChanges.ToArray(),
                Reason = translatedChanges.Reason,
                Operator = translatedChanges.Operator,
                OrganizationId = translatedChanges.Organization
            })
            .WithMessageId(messageId);

        var queue = new RoadNetworkCommandQueue(_store);
        await queue.Write(command, cancellationToken);

        return new LinkRoadSegmentToStreetNameResponse();
    }
}
