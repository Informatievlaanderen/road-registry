namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Extracts;
using Framework;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using System;
using RoadRegistry.Extensions;
using Uploads;

public class RoadRegistryContext : IRoadRegistryContext, IDisposable
{
    private readonly EventSourcedEntityMap _map;

    public RoadRegistryContext(
        EventSourcedEntityMap map,
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        JsonSerializerSettings settings,
        EventMapping mapping,
        EventEnricher eventEnricher,
        ILoggerFactory loggerFactory)
    {
        _map = map.ThrowIfNull();
        RoadNetworks = new RoadNetworks(map, store, snapshotReader, settings, mapping, loggerFactory.CreateLogger<RoadNetworks>());
        RoadNetworkExtracts = new RoadNetworkExtracts(map, store, settings, mapping, eventEnricher);
        RoadNetworkChangesArchives = new RoadNetworkChangesArchives(map, store, settings, mapping);
        Organizations = new Organizations(map, store, settings, mapping);
        EventFilter = new RoadRegistryEventFilter();
    }

    public IOrganizations Organizations { get; }
    public IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }
    public IRoadNetworkExtracts RoadNetworkExtracts { get; }
    public IRoadNetworks RoadNetworks { get; }
    public IRoadRegistryEventFilter EventFilter { get; }

    public void Dispose()
    {
        _map.Dispose();
    }
}
