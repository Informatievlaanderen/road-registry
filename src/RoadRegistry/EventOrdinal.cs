namespace RoadRegistry;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Monotonic counter that assigns each emitted domain event an ordinal reflecting its true emission order
// within a single road-network change. A single instance is shared across all aggregates participating in
// that change (via ScopedRoadNetworkChangeContext), so the cross-aggregate emission order is preserved.
// Marten's global seq_id cannot be relied upon for this: within one SaveChanges it flushes appends to
// existing streams before creating new streams, so created events (RoadNodeWasAdded, RoadSegmentWasAdded)
// end up with the highest seq_ids even though they were emitted first.
public interface IEventOrdinalProvider
{
    long Next();
}

public sealed class EventOrdinalProvider : IEventOrdinalProvider
{
    // Shared no-op provider for aggregates that are not participating in a change - rehydration from the event
    // stream, snapshot deserialization, migration. It always yields 0; a real emission order is only produced
    // once a change attaches its own provider via MartenAggregateRootEntity.AttachOrdinalProvider.
    public static IEventOrdinalProvider None { get; } = new NoOpEventOrdinalProvider();

    private long _next;

    public long Next() => _next++;

    private sealed class NoOpEventOrdinalProvider : IEventOrdinalProvider
    {
        public long Next() => 0;
    }
}

public static class EventOrdinal
{
    // Marten per-event header carrying the emission ordinal (see IEventOrdinalProvider). Stored as metadata
    // rather than on the event payload so the event hash / determinism stays intact.
    public const string HeaderKey = "roadNetworkChangeOrdinal";
}

// An uncommitted event together with the emission ordinal it was stamped with when added.
public readonly record struct RecordedEvent(IMartenEvent Event, long Ordinal);

// Ordered collection of an aggregate's uncommitted events. On Add it pulls the next ordinal from the
// aggregate's attached ordinal provider, capturing emission order at the moment each event is raised - by
// save time that order is already lost.
public sealed class UncommittedEventCollection : IEnumerable<IMartenEvent>
{
    private readonly IEventOrdinalProvider _ordinalProvider;
    private readonly List<RecordedEvent> _events = [];

    public UncommittedEventCollection(IEventOrdinalProvider ordinalProvider)
    {
        _ordinalProvider = ordinalProvider;
    }

    public void Add(IMartenEvent @event)
    {
        _events.Add(new RecordedEvent(@event, _ordinalProvider.Next()));
    }

    public int Count => _events.Count;
    public IMartenEvent this[int index] => _events[index].Event;
    public IReadOnlyList<RecordedEvent> Recorded => _events;
    public void Clear() => _events.Clear();

    public IEnumerator<IMartenEvent> GetEnumerator() => _events.Select(x => x.Event).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
