namespace RoadRegistry.Hosts;

using BackOffice;
using BackOffice.Core;
using BackOffice.Extensions;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using FluentValidation;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
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
            var translatedEx = ex.TranslateToDutch();

            //TODO-rik te bekijken of de ticketerror kan aangepast worden om een lijst van fouten op te slaan, ipv te moeten joinen
            var errorMessage = string.Join(",", translatedEx.Errors.Select(x => x.ErrorMessage));
            var errorCode = string.Join(",", translatedEx.Errors.Select(x => x.ErrorCode));
            Logger.LogError("InnerHandle failed with validation errors: {Message}", errorMessage);
            throw new RoadRegistryValidationException(errorMessage, errorCode);
        }
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

        return exception switch
        {
            RoadRegistryValidationException validationException => validationException.ToTicketError(),
            _ => null
        };
    }
}
