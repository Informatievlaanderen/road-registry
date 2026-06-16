namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.StreetName;

public sealed class FakeStreetNameClient : IStreetNameClient
{
    public Task<StreetNameItem?> GetAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult((StreetNameItem?)null);
    }
}
