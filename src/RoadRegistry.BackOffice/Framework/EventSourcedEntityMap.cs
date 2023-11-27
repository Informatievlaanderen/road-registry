namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class EventSourcedEntityMap: IDisposable
{
    private readonly ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry> _entries;

    public EventSourcedEntityMap()
    {
        var queue = new ConcurrentQueue<EventSourcedEntityMapEntry>();
        //TODO-rik fill up queue and return in `Entries` to keep order
        _entries = new ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry>();
    }

    public IEnumerable<EventSourcedEntityMapEntry> Entries => _entries.Values;

    public void Attach(EventSourcedEntityMapEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        if (!_entries.TryAdd(entry.Stream, entry)) throw new ArgumentException($"The event source of stream {entry.Stream} was already attached.");
    }

    public bool TryGet(StreamName stream, out EventSourcedEntityMapEntry entry)
    {
        return _entries.TryGetValue(stream, out entry);
    }

    public void Dispose()
    {
        _entries.Clear();

        GC.Collect();
    }
}
