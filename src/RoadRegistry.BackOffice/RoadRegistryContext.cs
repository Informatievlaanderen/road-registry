namespace RoadRegistry.BackOffice
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Core;
    using Framework;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using Uploads;

    public class RoadRegistryContext : IRoadRegistryContext
    {
        public RoadRegistryContext(EventSourcedEntityMap map, IStreamStore store,
            IRoadNetworkSnapshotReader snapshotReader, JsonSerializerSettings settings, EventMapping mapping)
        {
            RoadNetworks = new RoadNetworks(map, store, snapshotReader, settings, mapping);
            RoadNetworkChangesArchives = new RoadNetworkChangesArchives(map, store, settings, mapping);
            Organizations = new Organizations(map, store, settings, mapping);
        }

        public IRoadNetworks RoadNetworks { get; }
        public IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }
        public IOrganizations Organizations { get; }
    }
}
