namespace RoadRegistry.Hosts;

using BackOffice;
using BackOffice.Core;
using BackOffice.Exceptions;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using FluentValidation;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TicketingService.Abstractions;

public abstract class RoadRegistrySqsLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
    where TSqsLambdaRequest : SqsLambdaRequest
{
    protected RoadRegistrySqsLambdaHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ILogger logger)
        : base(retryPolicy, ticketing, idempotentCommandHandler)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(roadRegistryContext);
        ArgumentNullException.ThrowIfNull(logger);

        RoadRegistryContext = roadRegistryContext;
        DetailUrlFormat = options.DetailUrl;
        Logger = logger;
    }

    protected string DetailUrlFormat { get; }
    protected IRoadRegistryContext RoadRegistryContext { get; }
    protected ILogger Logger { get; }

    protected override TicketError MapValidationException(ValidationException exception, TSqsLambdaRequest request)
    {
        return base.MapValidationException(exception.TranslateToDutch(), request);
    }

    protected override async Task HandleAggregateIdIsNotFoundException(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await Ticketing.Error(request.TicketId, new RoadNetworkNotFound().ToTicketError(), cancellationToken);
    }

    protected virtual TicketError InnerMapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        return null;
    }

    protected sealed override TicketError MapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        var error = InnerMapDomainException(exception, request);
        if (error is not null)
        {
            return error;
        }

        var ticketError = exception switch
        {
            RoadRegistryValidationException validationException => validationException.ToTicketError(),
            RoadRegistryProblemsException problemsException => problemsException.ToTicketError(),
            _ => null
        };

        if (ticketError is not null)
        {
            if (Logger.IsEnabled(LogLevel.Information))
            {
                Logger.LogInformation("TicketError: {TicketError}", JsonSerializer.SerializeToElement(ticketError));
            }
        }


        return ticketError;
    }
}
