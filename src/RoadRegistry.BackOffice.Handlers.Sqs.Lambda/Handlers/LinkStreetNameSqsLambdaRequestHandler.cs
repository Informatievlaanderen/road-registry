namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Validation;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Exceptions;
using Extensions;
using Framework;
using Infrastructure;
using Messages;
using Microsoft.Extensions.Configuration;
using Requests;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public sealed class LinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<LinkStreetNameSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly IStreetNameCache _streetNameCache;

    public LinkStreetNameSqsLambdaRequestHandler(
        IConfiguration configuration,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IStreetNameCache streetNameCache,
        IRoadNetworkCommandQueue commandQueue)
        : base(
            configuration,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext)
    {
        _streetNameCache = streetNameCache;
        _commandQueue = commandQueue;
    }

    protected override async Task<ETagResponse> InnerHandle(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var roadSegmentId = request.Request.WegsegmentId;

        var command = await ToCommand(request, cancellationToken);
        var commandId = command.CreateCommandId();
        await _commandQueue.Write(new Command(command).WithMessageId(commandId), cancellationToken);

        try
        {
            await IdempotentCommandHandler.Dispatch(
                commandId,
                command,
                request.Metadata,
                cancellationToken);
        }
        catch (IdempotencyException)
        {
            // Idempotent: Do Nothing return last etag
        }

        var lastHash = await GetRoadSegmentHash(new RoadSegmentId(roadSegmentId), cancellationToken);
        return new ETagResponse(string.Format(DetailUrlFormat, roadSegmentId), lastHash);
    }
    
    private async Task<ChangeRoadNetwork> ToCommand(LinkStreetNameSqsLambdaRequest lambdaRequest, CancellationToken cancellationToken)
    {
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegment = roadNetwork.FindRoadSegment(new RoadSegmentId(lambdaRequest.Request.WegsegmentId));
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId(lambdaRequest.Provenance.Organisation.ToString()))
            .WithOperatorName(new OperatorName(lambdaRequest.Provenance.Operator))
            .WithReason(new Reason("Straatnaam koppelen"));

        var recordNumber = RecordNumber.Initial;

        var leftStreetNameId = lambdaRequest.Request.LinkerstraatnaamId.GetIdentifierFromPuri();
        var rightStreetNameId = lambdaRequest.Request.RechterstraatnaamId.GetIdentifierFromPuri();

        if (leftStreetNameId > 0)
        {
            if (!CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId))
            {
                throw new RoadRegistryValidationException(
                    ValidationErrors.RoadSegment.LeftStreetNameIsNotUnlinked.Message(lambdaRequest.Request.WegsegmentId),
                    ValidationErrors.RoadSegment.LeftStreetNameIsNotUnlinked.Code);
            }

            await ValidateStreetName(leftStreetNameId, cancellationToken);

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
                throw new RoadRegistryValidationException(
                    ValidationErrors.RoadSegment.RightStreetNameIsNotUnlinked.Message(lambdaRequest.Request.WegsegmentId),
                    ValidationErrors.RoadSegment.RightStreetNameIsNotUnlinked.Code);
            }

            await ValidateStreetName(rightStreetNameId, cancellationToken);

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
            throw new RoadRegistryValidationException(
                ValidationErrors.Common.IncorrectObjectId.Message(lambdaRequest.Request.LinkerstraatnaamId),
                ValidationErrors.Common.IncorrectObjectId.Code);
        }

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();
        var messageId = Guid.NewGuid();

        return new ChangeRoadNetwork(lambdaRequest.Provenance)
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(messageId)),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
    }

    protected override Task ValidateIfMatchHeaderValue(LinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ValidateStreetName(int streetNameId, CancellationToken cancellationToken)
    {
        var streetNameStatuses = await _streetNameCache.GetStreetNameStatusesById(new[] { streetNameId }, cancellationToken);
        if (!streetNameStatuses.TryGetValue(streetNameId, out var streetNameStatus))
        {
            throw new RoadRegistryValidationException(
                ValidationErrors.StreetName.NotFound.Message,
                ValidationErrors.StreetName.NotFound.Code);
        }

        if (!string.Equals(streetNameStatus, "proposed", StringComparison.InvariantCultureIgnoreCase)
            && !string.Equals(streetNameStatus, "current", StringComparison.InvariantCultureIgnoreCase))
        {
            throw new RoadRegistryValidationException(
                ValidationErrors.RoadSegment.StreetNameIsNotProposedOrCurrent.Message,
                ValidationErrors.RoadSegment.StreetNameIsNotProposedOrCurrent.Code);
        }
    }
}
