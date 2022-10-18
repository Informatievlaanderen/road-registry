namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class EventSourcedEntityMap
{
    public EventSourcedEntityMap()
    {
        _entries = new ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry>();
    }

    private readonly ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry> _entries;

    public void Attach(EventSourcedEntityMapEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        if (!_entries.TryAdd(entry.Stream, entry)) throw new ArgumentException($"The event source of stream {entry.Stream} was already attached.");
    }

    public IEnumerable<EventSourcedEntityMapEntry> Entries => _entries.Values;

    public bool TryGet(StreamName stream, out EventSourcedEntityMapEntry entry)
    {
        return _entries.TryGetValue(stream, out entry);
    }
}
