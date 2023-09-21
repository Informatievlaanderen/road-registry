namespace RoadRegistry.StreetName;

using System.Threading;
using System.Threading.Tasks;

public interface IStreetNameClient
{
    Task<StreetNameItem> GetAsync(int id, CancellationToken cancellationToken);
}
