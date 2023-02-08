namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
using Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using Requests;
using SqlStreamStore.Streams;

public sealed class CreateRoadNetworkSnapshotSqsLambdaRequestHandler : IRequestHandler<CreateRoadNetworkSnapshotSqsLambdaRequest>
{
    private readonly IRoadRegistryContext _context;
    private readonly IRoadNetworkSnapshotReader _snapshotReader;
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;
    private readonly RoadNetworkSnapshotStrategyOptions _snapshotStrategyOptions;
    private readonly ILogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler> _logger;
    private readonly Stopwatch _stopwatch;

    public CreateRoadNetworkSnapshotSqsLambdaRequestHandler(
        IRoadRegistryContext context,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        RoadNetworkSnapshotStrategyOptions snapshotStrategyOptions,
        ILogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler> logger)
    {
        _stopwatch = new Stopwatch();
        _context = context;
        _snapshotReader = snapshotReader;
        _snapshotWriter = snapshotWriter;
        _snapshotStrategyOptions = snapshotStrategyOptions;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateRoadNetworkSnapshotSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        try
        {
            var streamVersion = request.Request.StreamVersion; // Eg. 18499
            var streamDifference = streamVersion % _snapshotStrategyOptions.EventCount; // Eg. 49
            var streamMaxVersion = streamVersion - streamDifference; // Eg. 18450

            // Check if snapshot should be taken
            if (streamDifference.Equals(0))
            {
                var (snapshot, snapshotVersion) = await _snapshotReader.ReadSnapshot(cancellationToken);

                // Check if current snapshot is already further that stream version
                if (snapshotVersion >= streamMaxVersion)
                {
                    _logger.LogWarning("Create snapshot skipped for new message received from SQS with snapshot version {SnapshotVersion} and stream version {StreamVersion}", snapshotVersion, streamVersion);
                }
                else
                {
                    _logger.LogInformation("Create snapshot started for new message received from SQS with snapshot version {SnapshotVersion}", snapshotVersion, snapshotVersion);
                    await _snapshotWriter.WriteSnapshot(snapshot, snapshotVersion, cancellationToken);
                    _logger.LogInformation("Create snapshot completed for version {SnapshotVersion} in {TotalElapsedTimespan}", snapshotVersion, _stopwatch.Elapsed);

                    // TODO Signal that new version is available
                }
            }
            else
            {
                _logger.LogWarning("Snapshot strategy determined that the strategy limit had not been reached");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create snapshot failed in {TotalElapsedTimespan}", _stopwatch.Elapsed);
        }
        finally
        {
            _stopwatch.Stop();
        }

        return Unit.Value;
    }
}
