namespace RoadRegistry.BackOffice
{
    using Core;
    using Extracts;
    using Uploads;

    public interface IRoadRegistryContext
    {
        IRoadNetworks RoadNetworks { get; }

        IRoadNetworkExtracts RoadNetworkExtracts { get; }

        IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }

        IOrganizations Organizations { get; }
    }
}
