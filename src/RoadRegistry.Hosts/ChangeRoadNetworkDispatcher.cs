namespace RoadRegistry.Hosts;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;

public interface IChangeRoadNetworkDispatcher
{
    Task<ChangeRoadNetwork> DispatchAsync(SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken);
}

public class ChangeRoadNetworkDispatcher : IChangeRoadNetworkDispatcher
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly IIdempotentCommandHandler _idempotentCommandHandler;
    private readonly EventSourcedEntityMap _eventSourcedEntityMap;

    public ChangeRoadNetworkDispatcher(IRoadNetworkCommandQueue commandQueue, IIdempotentCommandHandler idempotentCommandHandler, EventSourcedEntityMap eventSourcedEntityMap)
    {
        _commandQueue = commandQueue;
        _idempotentCommandHandler = idempotentCommandHandler;
        _eventSourcedEntityMap = eventSourcedEntityMap;
    }

    public async Task<ChangeRoadNetwork> DispatchAsync(SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken)
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

        var messageId = lambdaRequest.TicketId;
        if (messageId == Guid.Empty)
        {
            messageId = Guid.NewGuid();
        }

        var changeRoadNetwork = new ChangeRoadNetwork(lambdaRequest.Provenance)
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(messageId)),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, cancellationToken);

        var commandId = changeRoadNetwork.CreateCommandId();
        await _commandQueue.Write(new Command(changeRoadNetwork).WithMessageId(commandId), cancellationToken);

        try
        {
            await _idempotentCommandHandler.Dispatch(
                commandId,
                changeRoadNetwork,
                lambdaRequest.Metadata,
                cancellationToken);
        }
        catch (IdempotencyException)
        {
            // Idempotent: Do Nothing return last etag
        }

        var entityMap = _eventSourcedEntityMap;

        var roadNetwork = entityMap.Entries.Single(x => x.Stream == RoadNetworks.Stream);
        foreach (var @event in roadNetwork.Entity.TakeEvents())
        {
            switch (@event)
            {
                case RoadNetworkChangesRejected roadNetworkChangesRejected:
                    var validationFailures = roadNetworkChangesRejected.Changes
                        .SelectMany(x => x.Problems)
                        .Select(problem => new ValidationFailure
                        {
                            PropertyName = "request",
                            ErrorCode = problem.Reason,
                            CustomState = problem.Parameters
                        });
                    throw new ValidationException(validationFailures);
            }
        }

        return changeRoadNetwork;
    }
}

public static class ChangeRoadNetworkDispatcherExtensions
{
    public static IServiceCollection AddChangeRoadNetworkDispatcher(this IServiceCollection services)
    {
        return services
            .AddSingleton<IChangeRoadNetworkDispatcher, ChangeRoadNetworkDispatcher>();
    }
}
