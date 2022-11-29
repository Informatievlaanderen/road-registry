namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Extensions;
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
    private readonly IStreetNameCache _streetNameCache;

    public LinkRoadSegmentToStreetNameRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<LinkRoadSegmentToStreetNameRequestHandler> logger,
        IStreamStore store,
        IRoadRegistryContext roadRegistryContext,
        IStreetNameCache streetNameCache)
        : base(dispatcher, logger)
    {
        _store = store;
        _roadRegistryContext = roadRegistryContext;
        _streetNameCache = streetNameCache;
    }

    public override async Task<LinkRoadSegmentToStreetNameResponse> HandleAsync(LinkRoadSegmentToStreetNameRequest request, CancellationToken cancellationToken)
    {
        var roadNetwork = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(request.RoadSegmentId));
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId("AGIV"))
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Straatnaam koppelen"));

        var recordNumber = RecordNumber.Initial;

        var leftStreetNameId = request.LeftStreetNameId.GetIdentifierFromPuri();
        var rightStreetNameId = request.RightStreetNameId.GetIdentifierFromPuri();

        if (leftStreetNameId > 0)
        {
            if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId))
            {
                throw ValidationError(nameof(request.LeftStreetNameId),
                    $"Het wegsegment '{request.RoadSegmentId}' heeft reeds een linkerstraatnaam. Gelieve deze eerst te ontkoppelen.",
                    "LinkerstraatnaamNietOntkoppeldValidatie");
            }

            await ValidateStreetName(nameof(request.LeftStreetNameId), leftStreetNameId, cancellationToken);

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
                new CrabStreetnameId(leftStreetNameId),
                roadSegment.AttributeHash.RightStreetNameId
            ).WithGeometry(roadSegment.Geometry));
        }
        else if (rightStreetNameId > 0)
        {
            if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId))
            {
                throw ValidationError(nameof(request.RightStreetNameId),
                    $"Het wegsegment '{request.RoadSegmentId}' heeft reeds een rechterstraatnaam. Gelieve deze eerst te ontkoppelen.",
                    "RechterstraatnaamNietOntkoppeldValidatie");
            }

            await ValidateStreetName(nameof(request.RightStreetNameId), rightStreetNameId, cancellationToken);

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
                new CrabStreetnameId(rightStreetNameId)
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

    private async Task ValidateStreetName(string propertyName, int streetNameId, CancellationToken cancellationToken)
    {
        var streetNameStatuses = await _streetNameCache.GetStreetNameStatusesById(new[] { streetNameId }, cancellationToken);
        if (streetNameStatuses.TryGetValue(streetNameId, out var streetNameStatus))
        {
            if (streetNameStatus != "Retired")
            {
                return;
            }

            throw ValidationError(propertyName,
                "De straatnaam is gehistoreerd of afgekeurd.",
                "WegsegmentStraatnaamGehistoreerdOfAfgekeurd");
        }

        throw ValidationError(propertyName,
            "De straatnaam is niet gekend in het Straatnamenregister.",
            "StraatnaamNietGekendValidatie");
    }

    private ValidationException ValidationError(string propertyName, string errorMessage, string errorCode = null)
    {
        return new ValidationException(new[]
        {
            new ValidationFailure(propertyName, errorMessage)
            {
                ErrorCode = errorCode
            }
        });
    }
}
