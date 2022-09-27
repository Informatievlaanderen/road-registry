namespace RoadRegistry.BackOffice.Framework;

using System;

public class EventSourcedEntityMapEntry
{
    public EventSourcedEntityMapEntry(IEventSourcedEntity source, StreamName stream, int expectedVersion)
    {
        Entity = source ?? throw new ArgumentNullException(nameof(source));
        Stream = stream;
        ExpectedVersion = expectedVersion;
    }

    public StreamName Stream { get; }
    public IEventSourcedEntity Entity { get; }
    public int ExpectedVersion { get; }
}
