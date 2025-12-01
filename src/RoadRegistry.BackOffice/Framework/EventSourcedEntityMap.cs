namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class EventSourcedEntityMap: IDisposable
{
    private readonly ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry> _entries = new();
    private readonly ConcurrentQueue<EventSourcedEntityMapEntry> _entriesQueue = new();

    public IEnumerable<EventSourcedEntityMapEntry> Entries => _entriesQueue;

    public void Attach(EventSourcedEntityMapEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        if (!_entries.TryAdd(entry.Stream, entry))
        {
            throw new ArgumentException($"The event source of stream {entry.Stream} was already attached.");
        }

        _entriesQueue.Enqueue(entry);
    }

    public bool TryGet(StreamName stream, out EventSourcedEntityMapEntry entry)
    {
        return _entries.TryGetValue(stream, out entry);
    }

    public void Dispose()
    {
        _entries.Clear();
        _entriesQueue.Clear();

        GC.Collect();
    }
}
