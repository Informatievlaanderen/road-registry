namespace RoadRegistry.BackOffice.Extracts;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkExtracts
{
    void Add(RoadNetworkExtract extract);
    Task<RoadNetworkExtract> Get(ExtractRequestId id, CancellationToken ct = default);
}
