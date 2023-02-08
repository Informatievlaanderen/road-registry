namespace RoadRegistry.Hosts;

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.Validation;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Hosts.Infrastructure.Extensions;
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

    protected abstract Task<ETagResponse> InnerHandleAsync(TSqsLambdaRequest request, CancellationToken cancellationToken);

    protected sealed override async Task<ETagResponse> InnerHandle(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            Logger.LogInformation("Started InnerHandle on {HandlerType} for lambda request {RequestType}", GetType().Name, request.GetType().Name);
            var result = await InnerHandleAsync(request, cancellationToken);
            Logger.LogInformation("Finished InnerHandle on {HandlerType} for lambda request {RequestType} in {StopwatchElapsedMilliseconds}", GetType().Name, request.GetType().Name, sw.ElapsedMilliseconds);
            return result;
        }
        catch (ValidationException ex)
        {
            var errorMessage = string.Join(",", ex.Errors.Select(x => x.ErrorMessage));
            var errorCode = string.Join(",", ex.Errors.Select(x => x.ErrorCode));
            throw new RoadRegistryValidationException(errorMessage, errorCode);
        }
    }

    protected override async Task HandleAggregateIdIsNotFoundException(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await Ticketing.Error(request.TicketId,
            new TicketError(
                ValidationErrors.RoadNetwork.NotFound.Message,
                ValidationErrors.RoadNetwork.NotFound.Code),
            cancellationToken);
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

        return exception switch
        {
            RoadRegistryValidationException validationException => validationException.ToTicketError(),
            _ => null
        };
    }
}
