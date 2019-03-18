namespace RoadRegistry.BackOffice
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;
    using Model;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using Translation;

    public class RoadRegistryContext : IRoadRegistryContext
    {
        public RoadRegistryContext(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        {
            RoadNetworks = new RoadNetworks(map, store, settings, mapping);
            RoadNetworkChangesArchives = new RoadNetworkChangesArchives(map, store, settings, mapping);
        }

        public IRoadNetworks RoadNetworks { get; }
        public IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }
    }
}
