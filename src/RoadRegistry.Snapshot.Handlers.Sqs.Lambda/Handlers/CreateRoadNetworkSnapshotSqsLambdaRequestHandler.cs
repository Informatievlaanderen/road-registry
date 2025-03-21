namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
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
    private readonly Stopwatch _stopwatch;

    public CreateRoadNetworkSnapshotSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IRoadRegistryContext context,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        ILoggerFactory loggerFactory)
        : base(options, retryPolicy, ticketing, context, loggerFactory.CreateLogger<CreateRoadNetworkSnapshotSqsLambdaRequestHandler>())
    {
        _stopwatch = new Stopwatch();
        _snapshotReader = snapshotReader;
        _snapshotWriter = snapshotWriter;
    }

    protected override async Task<object> InnerHandle(CreateRoadNetworkSnapshotSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        _stopwatch.Restart();

        try
        {
            var requestStreamVersion = request.Request.StreamVersion;
            var snapshotStreamVersion = await _snapshotReader.ReadSnapshotVersionAsync(cancellationToken);

            if (snapshotStreamVersion >= requestStreamVersion)
            {
                Logger.LogWarning("Create snapshot skipped for new message received from SQS with snapshot version {SnapshotVersion} and stream version {StreamVersion}", snapshotStreamVersion, requestStreamVersion);
                return new CreateRoadNetworkSnapshotResponse(null);
            }

            var (roadnetwork, roadnetworkVersion) = await RoadRegistryContext.RoadNetworks.GetWithVersion(
                true,
                cancelMessageProcessing: (_, _) => _stopwatch.Elapsed.Minutes >= 10,
                cancellationToken);
            var snapshot = roadnetwork.TakeSnapshot();

            Logger.LogInformation("Create snapshot started for new message received from SQS with snapshot version {SnapshotVersion}", roadnetworkVersion);
            await _snapshotWriter.WriteSnapshot(snapshot, roadnetworkVersion, cancellationToken);
            Logger.LogInformation("Create snapshot completed for version {SnapshotVersion} in {TotalElapsedTimespan}", roadnetworkVersion, _stopwatch.Elapsed);

            return new CreateRoadNetworkSnapshotResponse(roadnetworkVersion);
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
