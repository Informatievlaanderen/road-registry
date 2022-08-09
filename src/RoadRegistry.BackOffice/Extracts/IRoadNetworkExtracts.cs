namespace RoadRegistry.BackOffice.Extracts;

using System.Threading;
using System.Threading.Tasks;

public interface IRoadNetworkExtracts
{
    Task<RoadNetworkExtract> Get(ExtractRequestId id, CancellationToken ct = default);
    void Add(RoadNetworkExtract extract);
}
