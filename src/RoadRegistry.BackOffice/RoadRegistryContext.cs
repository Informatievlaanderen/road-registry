namespace RoadRegistry.BackOffice;

using Autofac.Core;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Extracts;
using Framework;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SqlStreamStore;
using Uploads;

public class RoadRegistryContext : IRoadRegistryContext
{
    private readonly ILogger<RoadRegistryContext> _logger;

    public RoadRegistryContext(
        EventSourcedEntityMap map,
        IStreamStore store,
        IRoadNetworkSnapshotReader snapshotReader,
        JsonSerializerSettings settings,
        EventMapping mapping,
        ILoggerFactory loggerFactory)
    {
        RoadNetworks = new RoadNetworks(map, store, snapshotReader, settings, mapping, loggerFactory.CreateLogger<RoadNetworks>());
        RoadNetworkExtracts = new RoadNetworkExtracts(map, store, settings, mapping);
        RoadNetworkChangesArchives = new RoadNetworkChangesArchives(map, store, settings, mapping);
        Organizations = new Organizations(map, store, settings, mapping);

        _logger = loggerFactory.CreateLogger<RoadRegistryContext>();
    }

    public IOrganizations Organizations { get; }
    public IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }
    public IRoadNetworkExtracts RoadNetworkExtracts { get; }
    public IRoadNetworks RoadNetworks { get; }
}
