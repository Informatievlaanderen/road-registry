namespace RoadRegistry.BackOffice
{
    using Core;
    using Uploads;

    public interface IRoadRegistryContext
    {
        IRoadNetworks RoadNetworks { get; }

        IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }

        IOrganizations Organizations { get; }
    }
}
