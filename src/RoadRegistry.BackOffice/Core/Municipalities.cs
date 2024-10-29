namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using System;

public class Municipalities : EventSourcedEntityRepository<Municipality, MunicipalityNisCode>, IMunicipalities
{
    public static readonly Func<MunicipalityNisCode, StreamName> ToStreamName = instance =>
        new StreamName(instance.ToString()).WithPrefix("municipality-");

    public Municipalities(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        : base(map, store, settings, mapping,
            ToStreamName,
            Municipality.Factory
        )
    {
    }

    protected override Municipality ConvertEntity(Municipality entity)
    {
        return entity.IsRemoved ? null : entity;
    }
}
