namespace RoadRegistry;

using System.Collections.Generic;
using Newtonsoft.Json;

public interface IMartenAggregateRootEntity
{
    string Id { get; }
    bool HasChanges();
    IReadOnlyList<RecordedEvent> GetRecordedChanges();
}

public abstract class MartenAggregateRootEntity<TIdentifier> : IMartenAggregateRootEntity
{
    [JsonIgnore]
    public string Id { get; set; } // Required for MartenDb

    public bool HasChanges() => _requestedToSaveSnapshot || UncommittedEvents.Count > 0;
    public IReadOnlyList<RecordedEvent> GetRecordedChanges() => UncommittedEvents.Recorded;

    // Stamps each raised event with its emission ordinal. Every aggregate is constructed with a provider
    // (EventOrdinalProvider.None when not participating in a change); a change swaps in its own provider via
    // AttachOrdinalProvider so the collection always has a non-null provider. Not persisted.
    protected UncommittedEventCollection UncommittedEvents { get; private set; }
    private bool _requestedToSaveSnapshot;

    protected MartenAggregateRootEntity(TIdentifier identifier, IEventOrdinalProvider ordinalProvider)
    {
        Id = StreamKeyFactory.Create(GetType(), identifier);
        UncommittedEvents = new UncommittedEventCollection(ordinalProvider);
    }

    // Swaps in the change's shared ordinal provider so events raised from now on are stamped in true
    // cross-aggregate emission order (see EventOrdinal). Called before any event is raised in the change, so the
    // (empty) collection is simply replaced. ScopedRoadNetwork overrides this to also propagate to its aggregates.
    internal virtual void AttachOrdinalProvider(IEventOrdinalProvider ordinalProvider)
    {
        UncommittedEvents = new UncommittedEventCollection(ordinalProvider);
    }

    public void RequestToSaveSnapshot()
    {
        _requestedToSaveSnapshot = true;
    }
}
