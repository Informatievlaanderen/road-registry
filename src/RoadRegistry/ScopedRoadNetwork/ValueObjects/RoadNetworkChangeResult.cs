namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using System.Collections;
using System.Collections.Generic;
using RoadRegistry.ValueObjects.Problems;

public sealed record RoadNetworkChangeResult(Problems Problems, RoadNetworkChangesSummary Summary);

public sealed class RoadNetworkChangesSummary
{
    public RoadNetworkEntityChangesSummary<RoadNodeId> RoadNodes { get; init; } = new();
    public RoadNetworkEntityChangesSummary<RoadSegmentId> RoadSegments { get; init; } = new();
    public RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> GradeSeparatedJunctions { get; init; } = new();
}

public sealed class RoadNetworkEntityChangesSummary<TIdentifier>
{
    public UniqueList<TIdentifier> Added { get; init; } = [];
    public UniqueList<TIdentifier> Modified { get; init;} = [];
    public UniqueList<TIdentifier> Removed { get; init; } = [];
}

public sealed class UniqueList<T> : IReadOnlyCollection<T>
{
    private readonly HashSet<T> _set = [];
    private readonly List<T> _list = [];

    public UniqueList()
    {
    }

    public UniqueList(IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            Add(item);
        }
    }

    public void Add(T item)
    {
        if (_set.Add(item))
        {
            _list.Add(item);
        }
    }

    public int Count => _list.Count;
    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
