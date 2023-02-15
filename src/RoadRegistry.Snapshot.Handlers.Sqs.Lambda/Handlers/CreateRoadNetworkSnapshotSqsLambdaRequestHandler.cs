namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using BackOffice;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Configuration;
using Hosts;
using Infrastructure;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Requests;
using SqlStreamStore;
using SqlStreamStore.Streams;
using TicketingService.Abstractions;

public sealed class CreateRoadNetworkSnapshotSqsLambdaRequestHandler : SqsLambdaHandler<CreateRoadNetworkSnapshotSqsLambdaRequest>
{
    private readonly IRoadNetworkSnapshotReader _snapshotReader;
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;
    private readonly IStreamStore _store;
    private readonly RoadNetworkSnapshotStrategyOptions _snapshotStrategyOptions;
    private readonly Stopwatch _stopwatch;

    public CreateRoadNetworkSnapshotSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IRoadRegistryContext context,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IStreamStore store,

        RoadNetworkSnapshotStrategyOptions snapshotStrategyOptions,
        ILogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler> logger)
        : base(options, retryPolicy, ticketing, null, context, logger)
    {
        _stopwatch = new Stopwatch();
        _snapshotReader = snapshotReader;
        _snapshotWriter = snapshotWriter;
        _store = store;
        _snapshotStrategyOptions = snapshotStrategyOptions;
    }

    protected override async Task<ETagResponse> InnerHandleAsync(CreateRoadNetworkSnapshotSqsLambdaRequest request, CancellationToken cancellationToken)
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
                var snapshotVersion = await _snapshotReader.ReadSnapshotVersionAsync(cancellationToken);

                // Check if current snapshot is already further that stream version
                if (snapshotVersion >= streamMaxVersion)
                {
                    Logger.LogWarning("Create snapshot skipped for new message received from SQS with snapshot version {SnapshotVersion} and stream version {StreamVersion}", snapshotVersion, streamVersion);
                }
                else
                {
                    var roadnetwork = await RoadRegistryContext.RoadNetworks.Get(streamMaxVersion, cancellationToken);
                    var snapshot = roadnetwork.TakeSnapshot();

                    Logger.LogInformation("Create snapshot started for new message received from SQS with snapshot version {SnapshotVersion}", streamMaxVersion);
                    await _snapshotWriter.WriteSnapshot(snapshot, streamMaxVersion, cancellationToken);
                    Logger.LogInformation("Create snapshot completed for version {SnapshotVersion} in {TotalElapsedTimespan}", streamMaxVersion, _stopwatch.Elapsed);
                }
            }
            else
            {
                Logger.LogWarning("Snapshot strategy determined that the strategy limit had not been reached");
            }
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
