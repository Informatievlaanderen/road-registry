namespace RoadRegistry.Hosts;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Abstractions.Exceptions;
using BackOffice.Abstractions.Organizations;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using BackOffice.Uploads;
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
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly EventSourcedEntityMap _eventSourcedEntityMap;
    private readonly ILogger<ChangeRoadNetworkDispatcher> _logger;
    private readonly OrganizationDbaseRecordReader _organizationRecordReader;

    public ChangeRoadNetworkDispatcher(
        IRoadNetworkCommandQueue commandQueue,
        IIdempotentCommandHandler idempotentCommandHandler,
        EditorContext editorContext,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        EventSourcedEntityMap eventSourcedEntityMap,
        ILogger<ChangeRoadNetworkDispatcher> logger)
    {
        _commandQueue = commandQueue;
        _idempotentCommandHandler = idempotentCommandHandler;
        _editorContext = editorContext;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _eventSourcedEntityMap = eventSourcedEntityMap;
        _logger = logger;
        _organizationRecordReader = new OrganizationDbaseRecordReader(_manager, _fileEncoding);
    }

    public async Task<ChangeRoadNetwork> DispatchAsync(SqsLambdaRequest lambdaRequest, string reason, Func<TranslatedChanges, Task<TranslatedChanges>> translatedChangesBuilder, CancellationToken cancellationToken)
    {
        var organizationRecords = await _editorContext.Organizations.ToListAsync(cancellationToken);
        var organizationDetails = organizationRecords.Select(organization => _organizationRecordReader.Read(organization.DbaseRecord, organization.DbaseSchemaVersion)).ToList();
        OrganizationDetail organizationDetail;

        if (OrganizationOvoCode.AcceptsValue(lambdaRequest.Provenance.Operator))
        {
            organizationDetail = organizationDetails.SingleOrDefault(sod => sod.OvoCode == new OrganizationOvoCode(lambdaRequest.Provenance.Operator.ToString()));

            if (organizationDetail is null)
            {
                var ex = new OrganizationOvoCodeNotFoundException(lambdaRequest.TicketId, lambdaRequest.Provenance);
                _logger.LogError(ex, ex.Message);

                organizationDetail = organizationDetails.SingleOrDefault(sod => sod.Code == new OrganizationId(lambdaRequest.Provenance.Operator.ToString()))
                                     ?? organizationDetails.Single(s => s.Code == new OrganizationId("AGIV"));
            }
        }
        else
        {
            organizationDetail = organizationDetails.SingleOrDefault(sod => sod.Code == new OrganizationId(lambdaRequest.Provenance.Operator.ToString()))
                ?? organizationDetails.Single(s => s.Code == new OrganizationId("AGIV"));
        }

        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(organizationDetail.Code)
            .WithOperatorName(new OperatorName(organizationDetail.Name))
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
