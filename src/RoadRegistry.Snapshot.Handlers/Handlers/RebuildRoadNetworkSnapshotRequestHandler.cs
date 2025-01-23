namespace RoadRegistry.Snapshot.Handlers.Handlers;

using System.Diagnostics;
using BackOffice;
using BackOffice.Abstractions.RoadNetworks;
using BackOffice.Core;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class RebuildRoadNetworkSnapshotRequestHandler : IRequestHandler<RebuildRoadNetworkSnapshotRequest, RebuildRoadNetworkSnapshotResponse>
{
    private readonly IRoadRegistryContext _context;
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;
    private readonly ILogger<RebuildRoadNetworkSnapshotRequestHandler> _logger;

    public RebuildRoadNetworkSnapshotRequestHandler(
        IRoadRegistryContext context,
        IRoadNetworkSnapshotWriter snapshotWriter,
        ILogger<RebuildRoadNetworkSnapshotRequestHandler> logger)
    {
        _context = context;
        _snapshotWriter = snapshotWriter;
        _logger = logger;
    }

    public async Task<RebuildRoadNetworkSnapshotResponse> Handle(RebuildRoadNetworkSnapshotRequest request, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();

        try
        {
            if (request.MaxStreamVersion > 0)
            {
                _logger.LogInformation("Loading messages until {MaxStreamVersion}", request.MaxStreamVersion);
            }
            else
            {
                _logger.LogInformation("Loading messages until latest");
            }

            var (roadNetwork, snapshotVersion) = await _context.RoadNetworks
                .GetWithVersion(
                    false,
                    cancelMessageProcessing: (messageStreamVersion, _) =>
                        request.MaxStreamVersion > 0 && messageStreamVersion > request.MaxStreamVersion,
                    cancellationToken);
            var snapshot = roadNetwork.TakeSnapshot();

            _logger.LogInformation("Create snapshot started with snapshot version {SnapshotVersion}", snapshotVersion);
            await _snapshotWriter.WriteSnapshot(snapshot, snapshotVersion, cancellationToken);
            _logger.LogInformation("Create snapshot completed for version {SnapshotVersion} in {TotalElapsedTimespan}", snapshotVersion, stopWatch.Elapsed);

            return new RebuildRoadNetworkSnapshotResponse(snapshotVersion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Create snapshot failed in {TotalElapsedTimespan}", stopWatch.Elapsed);
            throw;
        }
    }
}
