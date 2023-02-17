namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Framework;
using BackOffice.Messages;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;

public static class RoadNetworkCommandQueueExtensions
{
    public static async Task<ChangeRoadNetwork> DispatchChangeRoadNetwork(this IRoadNetworkCommandQueue commandQueue, IIdempotentCommandHandler idempotentCommandHandler, SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId(lambdaRequest.Provenance.Organisation.ToString()))
            .WithOperatorName(new OperatorName(lambdaRequest.Provenance.Operator))
            .WithReason(new Reason(reason));

        translatedChanges = await translatedChangesBuilder(translatedChanges);

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();
        var messageId = Guid.NewGuid();

        var command = new ChangeRoadNetwork(lambdaRequest.Provenance)
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(messageId)),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };

        var commandId = command.CreateCommandId();
        await commandQueue.Write(new Command(command).WithMessageId(commandId), cancellationToken);

        try
        {
            await idempotentCommandHandler.Dispatch(
                commandId,
                command,
                lambdaRequest.Metadata,
                cancellationToken);
        }
        catch (IdempotencyException)
        {
            // Idempotent: Do Nothing return last etag
        }

        return command;
    }
}
