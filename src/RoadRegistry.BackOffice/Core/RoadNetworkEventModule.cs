namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public class RoadNetworkEventModule : EventHandlerModule
{
    public RoadNetworkEventModule(
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter,
        IClock clock,
        ILogger<RoadNetworkEventModule> logger)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(snapshotWriter);

        For<CompletedRoadNetworkImport>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesAccepted));

                var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                logger.LogInformation("Event handler finished for {EventName}", nameof(CompletedRoadNetworkImport));
            });

        For<RoadNetworkChangesAccepted>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                logger.LogInformation("Event handler started for {EventName}", nameof(RoadNetworkChangesAccepted));

                var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);

                logger.LogInformation("Event handler finished for {EventName}", nameof(RoadNetworkChangesAccepted));
            });
    }
}
