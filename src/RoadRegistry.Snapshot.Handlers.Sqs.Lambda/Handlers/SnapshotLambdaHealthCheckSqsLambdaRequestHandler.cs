namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using System.Reflection;
using BackOffice;
using BackOffice.Core;
using BackOffice.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;

public sealed class SnapshotLambdaHealthCheckSqsLambdaRequestHandler : SqsLambdaHandler<SnapshotLambdaHealthCheckSqsLambdaRequest>
{
    private readonly IRoadNetworkSnapshotReader _snapshotReader;
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;

    public SnapshotLambdaHealthCheckSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IRoadRegistryContext context,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        ILogger<SnapshotLambdaHealthCheckSqsLambdaRequestHandler> logger)
        : base(options, retryPolicy, ticketing, context, logger)
    {
        _snapshotReader = snapshotReader;
        _snapshotWriter = snapshotWriter;
    }

    protected override async Task<object> InnerHandle(SnapshotLambdaHealthCheckSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        if (request.Request.AssemblyVersion != assemblyVersion)
        {
            throw new IncorrectAssemblyVersionException(assemblyVersion);
        }

        var snapshotVersion = await _snapshotReader.ReadSnapshotVersionAsync(cancellationToken);

        var (roadNetwork, roadNetworkVersion) = await RoadRegistryContext.RoadNetworks.GetWithVersion(true, (messageStreamVersion, _) => messageStreamVersion > snapshotVersion, cancellationToken);
        var snapshot = roadNetwork.TakeSnapshot();

        await _snapshotWriter.WriteSnapshot(snapshot, roadNetworkVersion, cancellationToken);

        return new ETagResponse(null, null);
    }

    protected override Task ValidateIfMatchHeaderValue(SnapshotLambdaHealthCheckSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
