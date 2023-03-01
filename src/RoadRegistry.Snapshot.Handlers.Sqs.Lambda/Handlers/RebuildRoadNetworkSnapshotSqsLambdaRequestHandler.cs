namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Configuration;
using System.Diagnostics;
using TicketingService.Abstractions;

public sealed class RebuildRoadNetworkSnapshotSqsLambdaRequestHandler : SqsLambdaHandler<RebuildRoadNetworkSnapshotSqsLambdaRequest>
{
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;
    private readonly RoadNetworkSnapshotStrategyOptions _snapshotStrategyOptions;
    private readonly Stopwatch _stopwatch;

    public RebuildRoadNetworkSnapshotSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IRoadRegistryContext context,
        IRoadNetworkSnapshotWriter snapshotWriter,
        RoadNetworkSnapshotStrategyOptions snapshotStrategyOptions,
        ILogger<RebuildRoadNetworkSnapshotSqsLambdaRequestHandler> logger)
        : base(options, retryPolicy, ticketing, null, context, logger)
    {
        _stopwatch = new Stopwatch();
        _snapshotWriter = snapshotWriter;
        _snapshotStrategyOptions = snapshotStrategyOptions;
    }
    
    protected override async Task<ETagResponse> InnerHandleAsync(RebuildRoadNetworkSnapshotSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        try
        {
            var (roadnetwork, snapshotVersion) = await RoadRegistryContext.RoadNetworks
                .GetWithVersion(false, (messageStreamVersion, pageLastStreamVersion) =>
                    messageStreamVersion > _snapshotStrategyOptions.GetLastAllowedStreamVersionToTakeSnapshot(pageLastStreamVersion)
                , cancellationToken);
            var snapshot = roadnetwork.TakeSnapshot();

            Logger.LogInformation("Create snapshot started for new message received from SQS with snapshot version {SnapshotVersion}", snapshotVersion);
            await _snapshotWriter.WriteSnapshot(snapshot, snapshotVersion, cancellationToken);
            Logger.LogInformation("Create snapshot completed for version {SnapshotVersion} in {TotalElapsedTimespan}", snapshotVersion, _stopwatch.Elapsed);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Create snapshot failed in {TotalElapsedTimespan}", _stopwatch.Elapsed);
        }
        finally
        {
            _stopwatch.Stop();
        }

        return new ETagResponse(string.Empty, string.Empty);
    }
}
