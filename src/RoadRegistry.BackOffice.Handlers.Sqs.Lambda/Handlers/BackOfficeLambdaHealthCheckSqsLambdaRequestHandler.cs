namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using System.Reflection;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Core;
using Exceptions;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;

public sealed class BackOfficeLambdaHealthCheckSqsLambdaRequestHandler : SqsLambdaHandler<BackOfficeLambdaHealthCheckSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLockOptions _distributedStreamStoreLockOptions;

    public BackOfficeLambdaHealthCheckSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        ILogger<BackOfficeLambdaHealthCheckSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _distributedStreamStoreLockOptions = distributedStreamStoreLockOptions;
    }

    protected override async Task<object> InnerHandle(BackOfficeLambdaHealthCheckSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (request.Request.AssemblyVersion != assemblyVersion)
        {
            throw new IncorrectAssemblyVersionException(assemblyVersion);
        }

        // try get lock in dynamo
        var distributedStreamStoreLock = new DistributedStreamStoreLock(_distributedStreamStoreLockOptions, RoadNetworkStreamNameProvider.Default, Logger);
        await distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(() => Task.CompletedTask, cancellationToken);

        // load snapshot
        var roadNetwork = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
        var outlinedRoadSegment = roadNetwork.FindRoadSegments(x => x.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined).FirstOrDefault();
        if (outlinedRoadSegment is not null)
        {
            // test db connection
            await RoadRegistryContext.RoadNetworks.ForOutlinedRoadSegment(outlinedRoadSegment.Id, cancellationToken);
        }

        return new ETagResponse(null, null);
    }

    protected override Task ValidateIfMatchHeaderValue(BackOfficeLambdaHealthCheckSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
