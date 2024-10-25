namespace RoadRegistry.BackOffice.Framework;

using System;

public abstract class EventSourcedEntity : IEventSourcedEntity
{
    private readonly EventPlayer Player = new();
    private readonly EventRecorder Recorder = new();

    private readonly EventEnricher? _eventEnricher;

    protected EventSourcedEntity()
    {
    }

    protected EventSourcedEntity(EventEnricher eventEnricher)
    {
        _eventEnricher = eventEnricher;
    }

    void IEventSourcedEntity.RestoreFromEvents(object[] events)
    {
        if (events == null)
            throw new ArgumentNullException(nameof(events));
        if (Recorder.HasRecordedEvents)
            throw new InvalidOperationException(
                "Restoring from events is not possible when an instance " +
                "has recorded events.");

        foreach (var @event in events)
            Player.Play(@event);
    }

    object[] IEventSourcedEntity.TakeEvents()
    {
        if (!Recorder.HasRecordedEvents)
            return Array.Empty<object>();

        var recorded = Recorder.RecordedEvents;
        return recorded;
    }

    protected void Apply(object @event)
    {
        _eventEnricher?.Invoke(@event);

        Player.Play(@event);
        Recorder.Record(@event);
    }

    protected void On<TEvent>(Action<TEvent> handler)
    {
        Player.Register(handler);
    }
}
