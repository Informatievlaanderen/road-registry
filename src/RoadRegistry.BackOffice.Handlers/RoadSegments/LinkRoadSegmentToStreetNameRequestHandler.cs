namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Shaperon;
using FluentValidation;
using FluentValidation.Results;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using SqlStreamStore;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public class LinkRoadSegmentToStreetNameRequestHandler : EndpointRequestHandler<LinkRoadSegmentToStreetNameRequest, LinkRoadSegmentToStreetNameResponse>
{
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly IStreamStore _store;

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
        var roadNetwork = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(request.RoadSegmentId));
        if (roadSegment == null)
        {
            throw ValidationError(nameof(request.RoadSegmentId), "Road segment does not exist.");
        }

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId("AGIV"))
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Straatnaam koppelen"));

        var recordNumber = RecordNumber.Initial;

        if (request.LeftStreetNameId > 0)
        {
            if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId))
            {
                throw ValidationError(nameof(request.RoadSegmentId), "Road segment is connected to a street name on the left side.");
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
                new CrabStreetnameId(request.LeftStreetNameId),
                roadSegment.AttributeHash.RightStreetNameId
            ).WithGeometry(roadSegment.Geometry));
        }
        else if (request.RightStreetNameId > 0)
        {
            if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId))
            {
                throw ValidationError(nameof(request.RoadSegmentId), "Road segment is connected to a street name on the right side.");
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
                new CrabStreetnameId(request.RightStreetNameId)
            ).WithGeometry(roadSegment.Geometry));
        }
        else
        {
            throw ValidationError(nameof(request.LeftStreetNameId), $"{nameof(request.LeftStreetNameId)} or {nameof(request.RightStreetNameId)} is required");
        }

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();
        var messageId = Guid.NewGuid();

        var command = new Command(new ChangeRoadNetwork
            {
                RequestId = ChangeRequestId.FromUploadId(new UploadId(messageId)),
                Changes = requestedChanges.ToArray(),
                Reason = translatedChanges.Reason,
                Operator = translatedChanges.Operator,
                OrganizationId = translatedChanges.Organization
            })
            .WithMessageId(messageId);

        var queue = new RoadNetworkCommandQueue(_store);
        await queue.Write(command, cancellationToken);

        return new LinkRoadSegmentToStreetNameResponse(messageId);
    }

    private ValidationException ValidationError(string propertyName, string errorMessage)
    {
        return new ValidationException("Validation error", new[] { new ValidationFailure(propertyName, errorMessage) });
    }
}
