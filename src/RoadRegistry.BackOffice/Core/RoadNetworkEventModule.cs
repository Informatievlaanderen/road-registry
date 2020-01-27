namespace RoadRegistry.BackOffice.Core
{
    using System;
    using Framework;
    using Messages;
    using NodaTime;
    using SqlStreamStore;

    public class RoadNetworkEventModule : EventHandlerModule
    {
        public RoadNetworkEventModule(IStreamStore store, IRoadNetworkSnapshotReader snapshotReader,
            IRoadNetworkSnapshotWriter snapshotWriter, IClock clock)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (clock == null) throw new ArgumentNullException(nameof(clock));
            if (snapshotReader == null) throw new ArgumentNullException(nameof(snapshotReader));
            if (snapshotWriter == null) throw new ArgumentNullException(nameof(snapshotWriter));

            For<CompletedRoadNetworkImport>()
                .UseRoadRegistryContext(store, snapshotReader, EnrichEvent.WithTime(clock))
                .Handle(async (context, message, ct) =>
                {
                    var (network, version) = await context.RoadNetworks.GetWithVersion(ct);
                    await snapshotWriter.WriteSnapshot(network.TakeSnapshot(), version, ct);
                });
        }
    }
}
