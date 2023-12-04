namespace RoadRegistry.Hosts;

using BackOffice;
using BackOffice.Abstractions.Organizations;
using BackOffice.Core;
using BackOffice.FeatureToggles;
using BackOffice.Framework;
using BackOffice.Messages;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Editor.Schema;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
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
    private readonly EditorContext _editorContext;
    private readonly EventSourcedEntityMap _eventSourcedEntityMap;
    private readonly UseOvoCodeInChangeRoadNetworkFeatureToggle _useOvoCodeInChangeRoadNetworkFeatureToggle;
    private readonly ILogger<ChangeRoadNetworkDispatcher> _logger;
    private readonly OrganizationDbaseRecordReader _organizationRecordReader;

    public ChangeRoadNetworkDispatcher(
        IRoadNetworkCommandQueue commandQueue,
        IIdempotentCommandHandler idempotentCommandHandler,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        EventSourcedEntityMap eventSourcedEntityMap,
        UseOvoCodeInChangeRoadNetworkFeatureToggle useOvoCodeInChangeRoadNetworkFeatureToggle,
        ILogger<ChangeRoadNetworkDispatcher> logger)
    {
        _commandQueue = commandQueue;
        _idempotentCommandHandler = idempotentCommandHandler;
        _editorContext = editorContext;
        _eventSourcedEntityMap = eventSourcedEntityMap;
        _useOvoCodeInChangeRoadNetworkFeatureToggle = useOvoCodeInChangeRoadNetworkFeatureToggle;
        _logger = logger;
        _organizationRecordReader = new OrganizationDbaseRecordReader(manager, fileEncoding);
    }

    public async Task<ChangeRoadNetwork> DispatchAsync(SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var organization = await GetOrganization(lambdaRequest.Provenance.Operator, cancellationToken);
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
        await _commandQueue.Write(new Command(changeRoadNetwork).WithMessageId(commandId), cancellationToken);

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

    private async Task<OrganizationDetail> GetOrganization(Operator @operator, CancellationToken cancellationToken)
    {
        if (OrganizationOvoCode.AcceptsValue(@operator))
        {
            if (_useOvoCodeInChangeRoadNetworkFeatureToggle.FeatureEnabled)
            {
                var organizationRecord = await _editorContext.Organizations.SingleOrDefaultAsync(x => x.Code == @operator, cancellationToken);
                if (organizationRecord is not null)
                {
                    return _organizationRecordReader.Read(organizationRecord.DbaseRecord, organizationRecord.DbaseSchemaVersion);
                }
            }
            else
            {
                var organizationRecords = await _editorContext.Organizations.ToListAsync(cancellationToken);
                var organizationDetails = organizationRecords.Select(organization => _organizationRecordReader.Read(organization.DbaseRecord, organization.DbaseSchemaVersion)).ToList();

                var ovoCode = new OrganizationOvoCode(@operator.ToString());
                var organizationDetail = organizationDetails.SingleOrDefault(sod => sod.OvoCode == ovoCode);
                if (organizationDetail is not null)
                {
                    return organizationDetail;
                }

                _logger.LogError($"Could not find a mapping to an organization for OVO-code {ovoCode}");
            }
        }

        return new OrganizationDetail
        {
            Code = new OrganizationId(@operator),
            Name = new OrganizationName(@operator)
        };
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
