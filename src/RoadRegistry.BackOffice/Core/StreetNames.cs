namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;

public class StreetNames : EventSourcedEntityRepository<StreetName, StreetNameId>, IStreetNames
{
    public StreetNames(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        : base(map, store, settings, mapping,
            StreetNameId.ToStreamName,
            StreetName.Factory
        )
    {
    }

    protected override StreetName ConvertEntity(StreetName entity)
    {
        return entity.IsRemoved ? null : entity;
    }
}
