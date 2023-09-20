namespace RoadRegistry.Snapshot.Handlers.Handlers;

using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions.RoadNetworks;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Core;

public sealed class RebuildRoadNetworkSnapshotRequestHandler : IRequestHandler<RebuildRoadNetworkSnapshotRequest, RebuildRoadNetworkSnapshotResponse>
{
    private readonly IRoadRegistryContext _context;
    private readonly IRoadNetworkSnapshotWriter _snapshotWriter;
    private readonly RoadNetworkSnapshotStrategyOptions _snapshotStrategyOptions;
    private readonly ILogger<RebuildRoadNetworkSnapshotRequestHandler> _logger;

    public RebuildRoadNetworkSnapshotRequestHandler(
        IRoadRegistryContext context,
        IRoadNetworkSnapshotWriter snapshotWriter,
        RoadNetworkSnapshotStrategyOptions snapshotStrategyOptions,
        ILogger<RebuildRoadNetworkSnapshotRequestHandler> logger)
    {
        _context = context;
        _snapshotWriter = snapshotWriter;
        _snapshotStrategyOptions = snapshotStrategyOptions;
        _logger = logger;
    }

    public async Task<RebuildRoadNetworkSnapshotResponse> Handle(RebuildRoadNetworkSnapshotRequest request, CancellationToken cancellationToken)
    {
        var stopWatch = Stopwatch.StartNew();

        try
        {
            var (roadNetwork, snapshotVersion) = await _context.RoadNetworks
                .GetWithVersion(false, (messageStreamVersion, pageLastStreamVersion) =>
                    {
                        if (request.MaxStreamVersion > 0 && messageStreamVersion > request.MaxStreamVersion)
                        {
                            return true;
                        }

                        return messageStreamVersion > _snapshotStrategyOptions.GetLastAllowedStreamVersionToTakeSnapshot(pageLastStreamVersion);
                    }
                    , cancellationToken);
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
