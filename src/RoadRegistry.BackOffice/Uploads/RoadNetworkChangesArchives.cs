namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Framework;
using Newtonsoft.Json;
using SqlStreamStore;

public class RoadNetworkChangesArchives : EventSourcedEntityRepository<RoadNetworkChangesArchive, ArchiveId>, IRoadNetworkChangesArchives
{
    public RoadNetworkChangesArchives(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
        : base(map, store, settings, mapping,
            ToStreamName,
            RoadNetworkChangesArchive.Factory
        )
    {
    }

    public static StreamName ToStreamName(ArchiveId id)
    {
        return new StreamName(id);
    }

    public void Add(RoadNetworkChangesArchive archive)
    {
        ArgumentNullException.ThrowIfNull(archive);

        Add(archive.Id, archive);
    }

    public Task<RoadNetworkChangesArchive> Get(ArchiveId id, CancellationToken ct = default)
    {
        return FindAsync(id, ct);
    }
}
