namespace RoadRegistry.RoadNetwork.ValueObjects;

using System.Collections;
using System.Collections.Generic;
using RoadRegistry.ValueObjects.Problems;

public sealed record RoadNetworkChangeResult(Problems Problems, RoadNetworkChangesSummary Summary);

public sealed class RoadNetworkChangesSummary
{
    public RoadNetworkEntityChangesSummary<RoadNodeId> RoadNodes { get; } = new();
    public RoadNetworkEntityChangesSummary<RoadSegmentId> RoadSegments { get; } = new();
    public RoadNetworkEntityChangesSummary<GradeSeparatedJunctionId> GradeSeparatedJunctions { get; } = new();
}

public sealed class RoadNetworkEntityChangesSummary<TIdentifier>
{
    public UniqueList<TIdentifier> Added { get; } = [];
    public UniqueList<TIdentifier> Modified { get; } = [];
    public UniqueList<TIdentifier> Removed { get; } = [];
}

public sealed class UniqueList<T> : IReadOnlyCollection<T>
{
    private readonly HashSet<T> _set = [];
    private readonly List<T> _list = [];

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
