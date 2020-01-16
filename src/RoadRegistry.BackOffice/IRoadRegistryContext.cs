namespace RoadRegistry.BackOffice
{
    using Model;
    using Translation;

    public interface IRoadRegistryContext
    {
        IRoadNetworks RoadNetworks { get; }

        IRoadNetworkChangesArchives RoadNetworkChangesArchives { get; }

        IOrganizations Organizations { get; }
    }
}
