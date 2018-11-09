namespace RoadRegistry.Framework
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    public class EventSourcedEntityMap
    {
        private readonly ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry> _entries;

        public EventSourcedEntityMap() => _entries = new ConcurrentDictionary<StreamName, EventSourcedEntityMapEntry>();

        public void Attach(EventSourcedEntityMapEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (!_entries.TryAdd(entry.Stream, entry))
                throw new ArgumentException($"The event source of stream {entry.Stream} was already attached.");
        }

        public bool TryGet(StreamName stream, out EventSourcedEntityMapEntry entry) => _entries.TryGetValue(stream, out entry);

        public EventSourcedEntityMapEntry[] Entries => _entries.Values.ToArray();
    }
}
