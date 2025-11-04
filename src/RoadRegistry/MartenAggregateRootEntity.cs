namespace RoadRegistry;

using System.Collections.Generic;
using System.Linq;

public interface IMartenAggregateRootEntity
{
    string Id { get; }
    bool HasChanges();
    IReadOnlyCollection<object> GetChanges();
}

public abstract class MartenAggregateRootEntity<TIdentifier> : IMartenAggregateRootEntity
{
    public string Id { get; set; } // Required for MartenDb

    public bool HasChanges() => _requestedToSaveSnapshot || UncommittedEvents.Any();
    public IReadOnlyCollection<object> GetChanges() => UncommittedEvents.AsReadOnly();

    protected List<object> UncommittedEvents { get; } = [];
    private bool _requestedToSaveSnapshot;

    protected MartenAggregateRootEntity(TIdentifier identifier)
    {
        Id = StreamKeyFactory.Create(GetType(), identifier);
    }

    public void RequestToSaveSnapshot()
    {
        _requestedToSaveSnapshot = true;
    }
}
