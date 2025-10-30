namespace RoadRegistry;

using System;
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

    protected List<object> UncommittedEvents { get; } = [];
    public bool HasChanges() => UncommittedEvents.Any();
    public IReadOnlyCollection<object> GetChanges() => UncommittedEvents.AsReadOnly();

    protected MartenAggregateRootEntity(TIdentifier identifier)
    {
        Id = StreamKeyFactory.Create(GetType(), identifier);
    }
}

public class StreamKeyFactory
{
    public static string Create<TIdentifier>(Type entityType, TIdentifier identifier)
    {
        return $"{entityType.Name}-{identifier}";
    }
}
