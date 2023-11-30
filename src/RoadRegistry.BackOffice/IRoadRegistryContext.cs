namespace RoadRegistry.BackOffice;

using Core;
using Extracts;
using Uploads;

public interface IRoadRegistryContext
{
    IOrganizations Organizations { get; }
    IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }
    IRoadNetworkExtracts RoadNetworkExtracts { get; }
    IRoadNetworks RoadNetworks { get; }
    IRoadRegistryEventFilter EventFilter { get; }
}
