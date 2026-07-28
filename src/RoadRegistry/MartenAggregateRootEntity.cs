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

    // The shared per-change ordinal provider used to stamp each raised event with its emission order. Every
    // aggregate must be constructed with one (EventOrdinalProvider.None when not participating in a change);
    // a change swaps in its own provider via AttachOrdinalProvider. Not persisted.
    private IEventOrdinalProvider _ordinalProvider;

    public bool HasChanges() => _requestedToSaveSnapshot || UncommittedEvents.Count > 0;
    public IReadOnlyList<RecordedEvent> GetRecordedChanges() => UncommittedEvents.Recorded;

    protected UncommittedEventCollection UncommittedEvents { get; }
    private bool _requestedToSaveSnapshot;

    protected MartenAggregateRootEntity(TIdentifier identifier, IEventOrdinalProvider ordinalProvider)
    {
        Id = StreamKeyFactory.Create(GetType(), identifier);
        _ordinalProvider = ordinalProvider;
        UncommittedEvents = new UncommittedEventCollection(() => _ordinalProvider);
    }

    // Swaps in the change's shared ordinal provider so events raised from now on are stamped in true
    // cross-aggregate emission order (see EventOrdinal). ScopedRoadNetwork overrides this to also propagate
    // the provider to every aggregate it holds.
    internal virtual void AttachOrdinalProvider(IEventOrdinalProvider ordinalProvider)
    {
        _ordinalProvider = ordinalProvider;
    }

    public void RequestToSaveSnapshot()
    {
        _requestedToSaveSnapshot = true;
    }
}
