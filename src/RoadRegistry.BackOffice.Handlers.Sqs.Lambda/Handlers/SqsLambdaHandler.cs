namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.Api.ETag;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Exceptions;
using FluentValidation;
using Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Infrastructure;
using TicketingService.Abstractions;

public abstract class SqsLambdaHandler<TSqsLambdaRequest> : SqsLambdaHandlerBase<TSqsLambdaRequest>
    where TSqsLambdaRequest : SqsLambdaRequest
{
    protected ILogger Logger { get; init; }

    protected SqsLambdaHandler(
        IConfiguration configuration,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        ILogger logger)
        : base(retryPolicy, ticketing, idempotentCommandHandler)
    {
        RoadRegistryContext = roadRegistryContext;
        DetailUrlFormat = configuration.GetRequiredValue<string>("DetailUrl");
        Logger = logger;
    }

    protected IRoadRegistryContext RoadRegistryContext { get; }
    protected string DetailUrlFormat { get; }

    protected async Task<string> GetRoadSegmentHash(
        RoadSegmentId roadSegmentId,
        CancellationToken cancellationToken)
    {
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegment = roadNetwork.FindRoadSegment(roadSegmentId);
        if (roadSegment == null)
        {
            throw new RoadSegmentNotFoundException();
        }

        return roadSegment.LastEventHash;
    }

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
                ValidationErrors.RoadSegment.NotFound.Message,
                ValidationErrors.RoadSegment.NotFound.Code),
            cancellationToken);
    }

    protected virtual TicketError? InnerMapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        return exception switch
        {
            RoadRegistryValidationException validationException => validationException.ToTicketError(),
            _ => null
        };
    }

    protected override TicketError? MapDomainException(DomainException exception, TSqsLambdaRequest request)
    {
        var error = InnerMapDomainException(exception, request);
        if (error is not null)
        {
            return error;
        }

        return exception switch
        {
            RoadRegistryValidationException validationException => validationException.ToTicketError(),
            RoadSegmentNotFoundException => new TicketError(
                ValidationErrors.RoadSegment.NotFound.Message,
                ValidationErrors.RoadSegment.NotFound.Code),
            _ => null
        };
    }

    protected override async Task ValidateIfMatchHeaderValue(TSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.IfMatchHeaderValue) || request is not IHasRoadSegmentId id)
        {
            return;
        }

        var latestEventHash = await GetRoadSegmentHash(
            new RoadSegmentId(id.RoadSegmentId),
            cancellationToken);

        var lastHashTag = new ETag(ETagType.Strong, latestEventHash);

        if (request.IfMatchHeaderValue != lastHashTag.ToString())
        {
            throw new IfMatchHeaderValueMismatchException();
        }
    }
}
