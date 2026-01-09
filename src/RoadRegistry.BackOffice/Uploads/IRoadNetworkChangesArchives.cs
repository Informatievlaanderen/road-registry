namespace RoadRegistry.BackOffice.Uploads;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkChangesArchives
{
    void Add(RoadNetworkChangesArchive archive);
    Task<RoadNetworkChangesArchive> Get(ArchiveId id, CancellationToken ct = default);
}
