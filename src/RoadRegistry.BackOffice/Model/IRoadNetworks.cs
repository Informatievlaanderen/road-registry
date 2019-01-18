namespace RoadRegistry.BackOffice.Model
{
    using System.Threading;
    using System.Threading.Tasks;

    public interface IRoadNetworks
    {
        Task<RoadNetwork> Get(CancellationToken ct = default);
    }
}
