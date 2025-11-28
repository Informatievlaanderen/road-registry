namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using RoadRegistry.RoadNetwork.ValueObjects;
using SqlStreamStore;

public class StreetNames : EventSourcedEntityRepository<StreetName, StreetNameLocalId>, IStreetNames
{
    public StreetNames(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        : base(map, store, settings, mapping,
            StreetNameLocalId.ToStreamName,
            StreetName.Factory
        )
    {
    }
}
