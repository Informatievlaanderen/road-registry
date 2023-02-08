namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

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
using Messages;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using RoadRegistry.Hosts;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public sealed class UnlinkStreetNameSqsLambdaRequestHandler : SqsLambdaHandler<UnlinkStreetNameSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;

    public UnlinkStreetNameSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkCommandQueue commandQueue,
        ILogger<UnlinkStreetNameSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _commandQueue = commandQueue;
    }

    protected override async Task<ETagResponse> InnerHandleAsync(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
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
    
    private async Task<ChangeRoadNetwork> ToCommand(UnlinkStreetNameSqsLambdaRequest lambdaRequest, CancellationToken cancellationToken)
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
            .WithReason(new Reason("Straatnaam ontkoppelen"));

        var recordNumber = RecordNumber.Initial;

        var leftStreetNameId = lambdaRequest.Request.LinkerstraatnaamId.GetIdentifierFromPuri();
        var rightStreetNameId = lambdaRequest.Request.RechterstraatnaamId.GetIdentifierFromPuri();

        if (leftStreetNameId > 0)
        {
            if (CrabStreetnameId.IsEmpty(roadSegment.AttributeHash.LeftStreetNameId) || (roadSegment.AttributeHash.LeftStreetNameId ?? 0) != leftStreetNameId)
            {
                throw new RoadRegistryValidationException(
                    ValidationErrors.RoadSegment.StreetName.Left.NotLinked.Message(lambdaRequest.Request.WegsegmentId, lambdaRequest.Request.LinkerstraatnaamId!),
                    ValidationErrors.RoadSegment.StreetName.Left.NotLinked.Code);
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
                throw new RoadRegistryValidationException(
                    ValidationErrors.RoadSegment.StreetName.Right.NotLinked.Message(lambdaRequest.Request.WegsegmentId, lambdaRequest.Request.RechterstraatnaamId!),
                    ValidationErrors.RoadSegment.StreetName.Right.NotLinked.Code);
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

    protected override Task ValidateIfMatchHeaderValue(UnlinkStreetNameSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
