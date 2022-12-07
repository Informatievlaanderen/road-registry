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

public class UnlinkStreetNameRequestHandler : EndpointRequestHandler<UnlinkStreetNameRequest, UnlinkStreetNameResponse>
{
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly IStreamStore _store;

    public UnlinkStreetNameRequestHandler(CommandHandlerDispatcher dispatcher,
        ILogger<UnlinkStreetNameRequestHandler> logger,
        IStreamStore store,
        IRoadRegistryContext roadRegistryContext)
        : base(dispatcher, logger)
    {
        _store = store;
        _roadRegistryContext = roadRegistryContext;
    }

    public override async Task<UnlinkStreetNameResponse> HandleAsync(UnlinkStreetNameRequest request, CancellationToken cancellationToken)
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
            .WithReason(new Reason("Straatnaam ontkoppelen"));

        var recordNumber = RecordNumber.Initial;

        var leftStreetNameId = request.LinkerstraatnaamId.GetIdentifierFromPuri();
        var rightStreetNameId = request.RechterstraatnaamId.GetIdentifierFromPuri();

        if (leftStreetNameId > 0)
        {
            if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId) || (roadSegment.AttributeHash.LeftStreetNameId ?? 0) != leftStreetNameId)
            {
                throw ValidationError(nameof(request.LinkerstraatnaamId),
                    ValidationErrors.RoadSegment.LeftStreetNameIsNotLinked.Message(request.WegsegmentId, request.LinkerstraatnaamId!),
                    ValidationErrors.RoadSegment.LeftStreetNameIsNotLinked.Code);
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
                null,
                roadSegment.AttributeHash.RightStreetNameId
            ).WithGeometry(roadSegment.Geometry));
        }
        else if (rightStreetNameId > 0)
        {
            if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.RightStreetNameId) || (roadSegment.AttributeHash.RightStreetNameId ?? 0) != rightStreetNameId)
            {
                throw ValidationError(nameof(request.RechterstraatnaamId),
                    ValidationErrors.RoadSegment.RightStreetNameIsNotLinked.Message(request.WegsegmentId, request.RechterstraatnaamId!),
                    ValidationErrors.RoadSegment.RightStreetNameIsNotLinked.Code);
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
                null
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

        return new UnlinkStreetNameResponse();
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
