namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Configuration;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Requests;
using TicketingService.Abstractions;

public sealed class CreateRoadNetworkSnapshotSqsLambdaRequestHandler : SqsLambdaHandler<CreateRoadNetworkSnapshotSqsLambdaRequest>
{
    private readonly IRoadNetworkSnapshotReader _snapshotReader;
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;
    private readonly RoadNetworkSnapshotStrategyOptions _snapshotStrategyOptions;
    private readonly Stopwatch _stopwatch;

    public CreateRoadNetworkSnapshotSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IRoadRegistryContext context,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        RoadNetworkSnapshotStrategyOptions snapshotStrategyOptions,
        ILogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler> logger)
        : base(options, retryPolicy, ticketing, null, context, logger)
    {
        _stopwatch = new Stopwatch();
        _snapshotReader = snapshotReader;
        _snapshotWriter = snapshotWriter;
        _snapshotStrategyOptions = snapshotStrategyOptions;
    }

    protected override async Task<object> InnerHandle(CreateRoadNetworkSnapshotSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        try
        {
            var streamVersion = request.Request.StreamVersion;
            var streamMaxVersion = _snapshotStrategyOptions.GetLastAllowedStreamVersionToTakeSnapshot(streamVersion);

            // Check if snapshot should be taken
            if (streamVersion.Equals(streamMaxVersion))
            {
                var snapshotVersion = await _snapshotReader.ReadSnapshotVersionAsync(cancellationToken);

                // Check if current snapshot is already further that stream version
                if (snapshotVersion >= streamMaxVersion)
                {
                    Logger.LogWarning("Create snapshot skipped for new message received from SQS with snapshot version {SnapshotVersion} and stream version {StreamVersion}", snapshotVersion, streamVersion);
                }
                else
                {
                    var (roadnetwork, roadnetworkVersion) = await RoadRegistryContext.RoadNetworks.GetWithVersion(true, (messageStreamVersion, _) => messageStreamVersion > streamMaxVersion, cancellationToken);
                    var snapshot = roadnetwork.TakeSnapshot();

                    Logger.LogInformation("Create snapshot started for new message received from SQS with snapshot version {SnapshotVersion}", roadnetworkVersion);
                    await _snapshotWriter.WriteSnapshot(snapshot, roadnetworkVersion, cancellationToken);
                    Logger.LogInformation("Create snapshot completed for version {SnapshotVersion} in {TotalElapsedTimespan}", roadnetworkVersion, _stopwatch.Elapsed);

                    return new CreateRoadNetworkSnapshotResponse(roadnetworkVersion);
                }
            }
            else
            {
                Logger.LogInformation("Snapshot strategy determined that the strategy limit had not been reached");
            }

            return new CreateRoadNetworkSnapshotResponse(null);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Create snapshot failed in {TotalElapsedTimespan}", _stopwatch.Elapsed);
            throw;
        }
        finally
        {
            _stopwatch.Stop();
        }
    }
}
