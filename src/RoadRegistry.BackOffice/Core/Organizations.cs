namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;
using System;

public class Organizations : EventSourcedEntityRepository<Organization, OrganizationId>, IOrganizations
{
    public static readonly Func<OrganizationId, StreamName> ToStreamName = OrganizationId.ToStreamName;

    public Organizations(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        : base(map, store, settings, mapping,
            ToStreamName,
            Organization.Factory
        )
    {
    }

    protected override Organization ConvertEntity(Organization entity)
    {
        return entity.IsRemoved || entity.Translation is null ? null : entity;
    }
}
