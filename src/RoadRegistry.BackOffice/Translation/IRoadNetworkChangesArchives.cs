namespace RoadRegistry.BackOffice.Translation
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRoadNetworkChangesArchives
    {
        Task<RoadNetworkChangesArchive> Get(ArchiveId id, CancellationToken ct = default);
        void Add(RoadNetworkChangesArchive archive);
    }
}
