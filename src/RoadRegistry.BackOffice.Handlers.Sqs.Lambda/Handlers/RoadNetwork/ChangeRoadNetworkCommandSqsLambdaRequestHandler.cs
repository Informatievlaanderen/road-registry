namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.RoadNetwork;

using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling.Actions.ChangeRoadNetwork;
using Core;
using Hosts;
using Infrastructure;
using Messages;
using Microsoft.Extensions.Logging;
using Requests.RoadNetwork;
using TicketingService.Abstractions;

public sealed class ChangeRoadNetworkCommandSqsLambdaRequestHandler : SqsLambdaHandler<ChangeRoadNetworkCommandSqsLambdaRequest>
{
    private readonly IExtractRequests _extractRequests;
    private readonly ChangeRoadNetworkCommandHandler _commandHandler;

    public ChangeRoadNetworkCommandSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IExtractRequests extractRequests,
        ChangeRoadNetworkCommandHandler commandHandler,
        ILoggerFactory loggerFactory)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            loggerFactory)
    {
        _commandHandler = commandHandler;
        _extractRequests = extractRequests;
    }

    protected override async Task<object> InnerHandle(ChangeRoadNetworkCommandSqsLambdaRequest sqsLambdaRequest, CancellationToken cancellationToken)
    {
        var command = sqsLambdaRequest.Request;

        await Ticketing.Pending(command.TicketId, cancellationToken);

        command.Provenance = sqsLambdaRequest.Provenance;

        var changeResult = await _commandHandler.Handle(command, cancellationToken);

        var downloadId = new DownloadId(command.DownloadId);
        var hasError = changeResult.Problems.HasError();
        if (hasError)
        {
            var errors = changeResult.Problems
                .Select(problem => problem.Translate().ToTicketError())
                .ToArray();

            await Ticketing.Error(command.TicketId, new TicketError(errors), cancellationToken);
            await _extractRequests.UploadRejectedAsync(downloadId, cancellationToken);
        }
        else
        {
            //TODO-pr ook resultaat van changes meegeven, bvb welke IDs zijn aangemaakt/gewijzigd/verwijderd, per entiteit
            //is al zeker nodig voor E2E testen
            await Ticketing.Complete(command.TicketId, new TicketResult(), cancellationToken);
            //TODO-pr aparte projectie voorzien voor resultaat dat extract details kan opvragen
            await _extractRequests.UploadAcceptedAsync(downloadId, cancellationToken);
        }

        return new object();
    }
}
