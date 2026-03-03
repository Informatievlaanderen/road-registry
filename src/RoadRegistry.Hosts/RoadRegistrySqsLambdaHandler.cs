namespace RoadRegistry.Hosts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Exceptions;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using FluentValidation;
using FluentValidation.Results;
using Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.DutchTranslations;
using TicketingService.Abstractions;
using ValueObjects.Problems;

public abstract class RoadRegistrySqsLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
    where TSqsLambdaRequest : SqsLambdaRequest
{
    protected IProblemTranslator ProblemTranslator { get; }

    protected RoadRegistrySqsLambdaHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ILoggerFactory loggerFactory,
        TicketingBehavior ticketingBehavior = TicketingBehavior.All,
        ProblemTranslatorBase? problemTranslator = null)
        : base(retryPolicy, ticketing, idempotentCommandHandler, ticketingBehavior)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(roadRegistryContext);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        ProblemTranslator = problemTranslator ?? WellKnownProblemTranslators.Default;
        RoadRegistryContext = roadRegistryContext;
        DetailUrlFormat = options.DetailUrl;
        Logger = loggerFactory.CreateLogger(GetType());
    }

    protected RoadRegistrySqsLambdaHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ILogger logger,
        TicketingBehavior ticketingBehavior = TicketingBehavior.All,
        ProblemTranslatorBase? problemTranslator = null)
        : base(retryPolicy, ticketing, idempotentCommandHandler, ticketingBehavior)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(roadRegistryContext);
        ArgumentNullException.ThrowIfNull(logger);

        ProblemTranslator = problemTranslator ?? WellKnownProblemTranslators.Default;
        RoadRegistryContext = roadRegistryContext;
        DetailUrlFormat = options.DetailUrl;
        Logger = logger;
    }

    protected string DetailUrlFormat { get; }
    protected IRoadRegistryContext RoadRegistryContext { get; }
    protected ILogger Logger { get; }

    protected override TicketError MapValidationException(ValidationException exception, TSqsLambdaRequest request)
    {
        exception = exception.TranslateToDutch(ProblemTranslator);

        if (exception.Errors is not null)
        {
            return exception.Errors.Count() == 1
                ? ToTicketError(exception.Errors.Single())
                : new TicketError(exception.Errors.Select(ToTicketError).ToList());
        }

        return null;
    }

    private static TicketError ToTicketError(ValidationFailure failure)
    {
        return new TicketError(failure.ErrorMessage, failure.ErrorCode)
        {
            ErrorContext = failure.CustomState as Dictionary<string, object>
        };
    }

    protected override async Task HandleAggregateIdIsNotFoundException(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await Ticketing.Error(request.TicketId, new RoadNetworkNotFound().ToTicketError(ProblemTranslator), cancellationToken);
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
            //DutchValidationException validationException => validationException.ToTicketError(),
            RoadRegistryProblemsException problemsException => problemsException.ToTicketError(ProblemTranslator),
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
