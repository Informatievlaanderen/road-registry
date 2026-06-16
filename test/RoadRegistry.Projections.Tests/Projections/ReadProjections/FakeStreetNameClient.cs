namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.StreetName;

/// <summary>
/// Minimal <see cref="IStreetNameClient"/> test double. Returns names only for ids that were
/// explicitly added; otherwise <c>null</c> (mirroring "not found"). The read projection only
/// consults the client when the cache misses, so most tests seed <see cref="FakeStreetNameCache"/>
/// and this stub is never hit.
/// </summary>
public sealed class FakeStreetNameClient : IStreetNameClient
{
    private readonly Dictionary<int, StreetNameItem> _items = new();

    public FakeStreetNameClient Add(int id, string name)
    {
        _items[id] = new StreetNameItem { Id = id, Name = name };
        return this;
    }

    public Task<StreetNameItem> GetAsync(int id, CancellationToken cancellationToken)
    {
        return Task.FromResult(_items.TryGetValue(id, out var item) ? item : null);
    }
}
