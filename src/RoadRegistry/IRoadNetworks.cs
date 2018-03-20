using System.Threading;
using System.Threading.Tasks;

namespace RoadRegistry
{
    public interface IRoadNetworks
    {
        Task<RoadNetwork> Get(CancellationToken ct = default);
    }
}