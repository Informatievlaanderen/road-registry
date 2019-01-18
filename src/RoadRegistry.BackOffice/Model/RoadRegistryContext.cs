namespace RoadRegistry.BackOffice.Model
{
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;
    using Newtonsoft.Json;
    using SqlStreamStore;

    public class RoadRegistryContext : IRoadRegistryContext
    {
        public RoadRegistryContext(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        {
            RoadNetworks = new RoadNetworks(map, store, settings, mapping);
        }

        public IRoadNetworks RoadNetworks { get; }
    }
}
