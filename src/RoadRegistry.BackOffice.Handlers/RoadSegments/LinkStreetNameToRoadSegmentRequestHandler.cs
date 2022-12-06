namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Abstractions.Validation;
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

public class LinkStreetNameToRoadSegmentRequestHandler : EndpointRequestHandler<LinkStreetNameToRoadSegmentRequest, LinkStreetNameToRoadSegmentResponse>
{
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly IStreamStore _store;
    private readonly IStreetNameCache _streetNameCache;

    public LinkStreetNameToRoadSegmentRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<LinkStreetNameToRoadSegmentRequestHandler> logger,
        IStreamStore store,
        IRoadRegistryContext roadRegistryContext,
        IStreetNameCache streetNameCache)
        : base(dispatcher, logger)
    {
        _store = store;
        _roadRegistryContext = roadRegistryContext;
        _streetNameCache = streetNameCache;
    }

    public override async Task<LinkStreetNameToRoadSegmentResponse> HandleAsync(LinkStreetNameToRoadSegmentRequest request, CancellationToken cancellationToken)
    {
        var roadNetwork = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(request.WegsegmentId));
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId("AGIV"))
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Straatnaam koppelen"));

        var recordNumber = RecordNumber.Initial;

        var leftStreetNameId = request.LinkerstraatnaamId.GetIdentifierFromPuri();
        var rightStreetNameId = request.RechterstraatnaamId.GetIdentifierFromPuri();

        if (leftStreetNameId > 0)
        {
            if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId))
            {
                throw ValidationError(nameof(request.LinkerstraatnaamId),
                    ValidationErrors.RoadSegment.LeftStreetNameIsNotUnlinked.Message(request.WegsegmentId),
                    ValidationErrors.RoadSegment.LeftStreetNameIsNotUnlinked.Code);
            }

            await ValidateStreetName(nameof(request.LinkerstraatnaamId), leftStreetNameId, cancellationToken);

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
                throw ValidationError(nameof(request.RechterstraatnaamId),
                    ValidationErrors.RoadSegment.RightStreetNameIsNotUnlinked.Message(request.WegsegmentId),
                    ValidationErrors.RoadSegment.RightStreetNameIsNotUnlinked.Code);
            }

            await ValidateStreetName(nameof(request.RechterstraatnaamId), rightStreetNameId, cancellationToken);

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
            throw ValidationError(nameof(request.LinkerstraatnaamId),
                ValidationErrors.Common.IncorrectObjectId.Message(request.LinkerstraatnaamId),
                ValidationErrors.Common.IncorrectObjectId.Code);
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

        return new LinkStreetNameToRoadSegmentResponse();
    }

    private async Task ValidateStreetName(string propertyName, int streetNameId, CancellationToken cancellationToken)
    {
        var streetNameStatuses = await _streetNameCache.GetStreetNameStatusesById(new[] { streetNameId }, cancellationToken);
        if (!streetNameStatuses.TryGetValue(streetNameId, out var streetNameStatus))
        {
            throw ValidationError(propertyName,
                ValidationErrors.StreetName.NotFound.Message,
                ValidationErrors.StreetName.NotFound.Code);
        }

        if (!string.Equals(streetNameStatus,"proposed", StringComparison.InvariantCultureIgnoreCase)
            && !string.Equals(streetNameStatus, "current", StringComparison.InvariantCultureIgnoreCase))
        {
            throw ValidationError(propertyName,
                ValidationErrors.RoadSegment.StreetNameIsNotProposedOrCurrent.Message,
                ValidationErrors.RoadSegment.StreetNameIsNotProposedOrCurrent.Code);
        }
    }

    private ValidationException ValidationError(string propertyName, string errorMessage, string errorCode)
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
