namespace RoadRegistry.Hosts;

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
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reason = BackOffice.Reason;

public interface IChangeRoadNetworkDispatcher
{
    Task<ChangeRoadNetwork> DispatchAsync(SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken);
}

public class ChangeRoadNetworkDispatcher : IChangeRoadNetworkDispatcher
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly IIdempotentCommandHandler _idempotentCommandHandler;
    private readonly EventSourcedEntityMap _eventSourcedEntityMap;
    private readonly IOrganizationRepository _organizationRepository;
    private readonly ILogger<ChangeRoadNetworkDispatcher> _logger;

    public ChangeRoadNetworkDispatcher(
        IRoadNetworkCommandQueue commandQueue,
        IIdempotentCommandHandler idempotentCommandHandler,
        EventSourcedEntityMap eventSourcedEntityMap,
        IOrganizationRepository organizationRepository,
        ILogger<ChangeRoadNetworkDispatcher> logger)
    {
        _commandQueue = commandQueue;
        _idempotentCommandHandler = idempotentCommandHandler;
        _eventSourcedEntityMap = eventSourcedEntityMap;
        _organizationRepository = organizationRepository;
        _logger = logger;
    }

    public async Task<ChangeRoadNetwork> DispatchAsync(SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var organizationId = new OrganizationId(lambdaRequest.Provenance.Operator);
        var organization = await _organizationRepository.FindByIdOrOvoCodeAsync(organizationId, cancellationToken)
                           ?? OrganizationDetail.FromCode(organizationId);
        _logger.LogInformation("TIMETRACKING dispatcher: finding organization by '{Operator}' took {Elapsed}", lambdaRequest.Provenance.Operator, sw.Elapsed);
        sw.Restart();

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(organization.Code)
            .WithOperatorName(new OperatorName(organization.Name))
            .WithReason(new Reason(reason));

        translatedChanges = await translatedChangesBuilder(translatedChanges);
        _logger.LogInformation("TIMETRACKING dispatcher: building TranslatedChanges took {Elapsed}",  sw.Elapsed);

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
        sw.Restart();
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, cancellationToken);
        _logger.LogInformation("TIMETRACKING dispatcher: validating ChangeRoadNetwork took {Elapsed}", sw.Elapsed);

        var commandId = changeRoadNetwork.CreateCommandId();
        await _commandQueue.WriteAsync(new Command(changeRoadNetwork).WithMessageId(commandId), cancellationToken);

        try
        {
            sw.Restart();
            await _idempotentCommandHandler.Dispatch(
                commandId,
                changeRoadNetwork,
                lambdaRequest.Metadata,
                cancellationToken);
            _logger.LogInformation("TIMETRACKING dispatcher: dispatching ChangeRoadNetwork took {Elapsed}", sw.Elapsed);
        }
        catch (IdempotencyException)
        {
            // Idempotent: Do Nothing return last etag
        }

        var entityMap = _eventSourcedEntityMap;

        var roadNetworkEvents = entityMap.Entries
            .Select(x => x.Entity)
            .Where(x => x is RoadNetwork)
            .SelectMany(x => x.TakeEvents())
            .ToList();
        foreach (var @event in roadNetworkEvents)
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
