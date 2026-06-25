namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RoadRegistry.StreetName;

public sealed class FakeStreetNameClient : IStreetNameClient
{
    private readonly Dictionary<int, string> _names = new();
    private Exception? _throwOnGet;

    public FakeStreetNameClient WithStreetName(int id, string name)
    {
        _names[id] = name;
        return this;
    }

    public FakeStreetNameClient ThatThrows(Exception exception)
    {
        _throwOnGet = exception;
        return this;
    }

    public Task<StreetNameItem?> GetAsync(int id, CancellationToken cancellationToken)
    {
        if (_throwOnGet is not null)
            throw _throwOnGet;

        if (_names.TryGetValue(id, out var name))
            return Task.FromResult<StreetNameItem?>(new StreetNameItem { Id = id, Name = name });

        return Task.FromResult((StreetNameItem?)null);
    }
}
