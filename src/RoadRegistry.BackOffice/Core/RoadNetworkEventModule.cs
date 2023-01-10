namespace RoadRegistry.BackOffice.Core;

using System;
using Framework;
using Messages;
using NodaTime;
using NodaTime.TimeZones;
using SqlStreamStore;

public class RoadNetworkEventModule : EventHandlerModule
{
    public RoadNetworkEventModule(IStreamStore store, IRoadNetworkSnapshotReader snapshotReader,
        IRoadNetworkSnapshotWriter snapshotWriter, IClock clock)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(snapshotReader);
        ArgumentNullException.ThrowIfNull(snapshotWriter);

        For<CompletedRoadNetworkImport>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);
            });

        For<RoadNetworkChangesAccepted>()
            .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
            .Handle(async (context, message, ct) =>
            {
                var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);
            });
    }
}
