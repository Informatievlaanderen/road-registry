namespace RoadRegistry.Projections.Tests;

using JasperFx.Events;
using Marten.Events;
using Marten.Linq;

public class InMemoryEventStoreOperations : IEventStoreOperations
{
    private readonly List<StreamAction> _streamActions = [];
    private readonly HashSet<object> _streamKeys = [];

    public IReadOnlyCollection<StreamAction> GetAndClearStreamActions()
    {
        var result = _streamActions.ToList().AsReadOnly();
        _streamActions.Clear();
        return result;
    }

    public StreamAction Append(Guid stream, IEnumerable<object> events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction Append(Guid stream, params object[] events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction Append(string stream, IEnumerable<object> events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction Append(string stream, params object[] events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction Append(Guid stream, long expectedVersion, params object[] events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction Append(string stream, long expectedVersion, IEnumerable<object> events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction Append(string stream, long expectedVersion, params object[] events)
    {
        _streamKeys.Add(stream);
        _streamActions.Add(new StreamAction(stream, StreamActionType.Append)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream<TAggregate>(Guid id, params object[] events) where TAggregate : class
    {
        if (!_streamKeys.Add(id))
        {
            throw new InvalidOperationException($"Stream {id} already exists");
        }

        _streamActions.Add(new StreamAction(id, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(Type aggregateType, Guid id, IEnumerable<object> events)
    {
        if (!_streamKeys.Add(id))
        {
            throw new InvalidOperationException($"Stream {id} already exists");
        }

        _streamActions.Add(new StreamAction(id, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(Type aggregateType, Guid id, params object[] events)
    {
        if (!_streamKeys.Add(id))
        {
            throw new InvalidOperationException($"Stream {id} already exists");
        }

        _streamActions.Add(new StreamAction(id, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream<TAggregate>(string streamKey, IEnumerable<object> events) where TAggregate : class
    {
        if (!_streamKeys.Add(streamKey))
        {
            throw new InvalidOperationException($"Stream {streamKey} already exists");
        }

        _streamActions.Add(new StreamAction(streamKey, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream<TAggregate>(string streamKey, params object[] events) where TAggregate : class
    {
        if (!_streamKeys.Add(streamKey))
        {
            throw new InvalidOperationException($"Stream {streamKey} already exists");
        }

        _streamActions.Add(new StreamAction(streamKey, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(Type aggregateType, string streamKey, IEnumerable<object> events)
    {
        if (!_streamKeys.Add(streamKey))
        {
            throw new InvalidOperationException($"Stream {streamKey} already exists");
        }

        _streamActions.Add(new StreamAction(streamKey, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(Type aggregateType, string streamKey, params object[] events)
    {
        if (!_streamKeys.Add(streamKey))
        {
            throw new InvalidOperationException($"Stream {streamKey} already exists");
        }

        _streamActions.Add(new StreamAction(streamKey, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(Guid id, IEnumerable<object> events)
    {
        if (!_streamKeys.Add(id))
        {
            throw new InvalidOperationException($"Stream {id} already exists");
        }

        _streamActions.Add(new StreamAction(id, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(Guid id, params object[] events)
    {
        if (!_streamKeys.Add(id))
        {
            throw new InvalidOperationException($"Stream {id} already exists");
        }

        _streamActions.Add(new StreamAction(id, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(string streamKey, IEnumerable<object> events)
    {
        if (!_streamKeys.Add(streamKey))
        {
            throw new InvalidOperationException($"Stream {streamKey} already exists");
        }

        _streamActions.Add(new StreamAction(streamKey, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream(string streamKey, params object[] events)
    {
        if (!_streamKeys.Add(streamKey))
        {
            throw new InvalidOperationException($"Stream {streamKey} already exists");
        }

        _streamActions.Add(new StreamAction(streamKey, StreamActionType.Start)
            .AddEvents(events.Select(Event.For).ToList()));
        return _streamActions.Last();
    }

    public StreamAction StartStream<TAggregate>(IEnumerable<object> events) where TAggregate : class
    {
        throw new NotImplementedException();
    }

    public StreamAction StartStream<TAggregate>(params object[] events) where TAggregate : class
    {
        throw new NotImplementedException();
    }

    public StreamAction StartStream(Type aggregateType, IEnumerable<object> events)
    {
        throw new NotImplementedException();
    }

    public StreamAction StartStream(Type aggregateType, params object[] events)
    {
        throw new NotImplementedException();
    }

    public StreamAction StartStream(IEnumerable<object> events)
    {
        throw new NotImplementedException();
    }

    public StreamAction StartStream(params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task CompactStreamAsync<T>(string streamKey, Action<StreamCompactingRequest<T>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public Task CompactStreamAsync<T>(Guid streamId, Action<StreamCompactingRequest<T>>? configure = null)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<IEvent>> FetchStreamAsync(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, long fromVersion = 0, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<IEvent>> FetchStreamAsync(string streamKey, long version = 0, DateTimeOffset? timestamp = null, long fromVersion = 0, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<T?> AggregateStreamAsync<T>(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, T? state = default(T?), long fromVersion = 0, CancellationToken token = new CancellationToken()) where T : class
    {
        return Task.FromResult((T?)null);
    }

    public Task<T?> AggregateStreamAsync<T>(string streamKey, long version = 0, DateTimeOffset? timestamp = null, T? state = default(T?), long fromVersion = 0, CancellationToken token = new CancellationToken()) where T : class
    {
        return Task.FromResult((T?)null);
    }

    public Task<T?> AggregateStreamToLastKnownAsync<T>(Guid streamId, long version = 0, DateTimeOffset? timestamp = null, CancellationToken token = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<T?> AggregateStreamToLastKnownAsync<T>(string streamKey, long version = 0, DateTimeOffset? timestamp = null, CancellationToken token = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<T> QueryRawEventDataOnly<T>() where T : notnull
    {
        throw new NotImplementedException();
    }

    public IMartenQueryable<IEvent> QueryAllRawEvents()
    {
        throw new NotImplementedException();
    }

    public Task<IEvent<T>?> LoadAsync<T>(Guid id, CancellationToken token = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IEvent?> LoadAsync(Guid id, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<StreamState?> FetchStreamStateAsync(Guid streamId, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public Task<StreamState?> FetchStreamStateAsync(string streamKey, CancellationToken token = new CancellationToken())
    {
        throw new NotImplementedException();
    }

    public IEvent BuildEvent(object data)
    {
        throw new NotImplementedException();
    }

    public StreamAction Append(Guid stream, long expectedVersion, IEnumerable<object> events)
    {
        throw new NotImplementedException();
    }

    public StreamAction StartStream<TAggregate>(Guid id, IEnumerable<object> events) where TAggregate : class
    {
        throw new NotImplementedException();
    }

    public Task AppendOptimistic(string streamKey, CancellationToken token, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendOptimistic(string streamKey, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendOptimistic(Guid streamId, CancellationToken token, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendOptimistic(Guid streamId, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendExclusive(string streamKey, CancellationToken token, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendExclusive(string streamKey, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendExclusive(Guid streamId, CancellationToken token, params object[] events)
    {
        throw new NotImplementedException();
    }

    public Task AppendExclusive(Guid streamId, params object[] events)
    {
        throw new NotImplementedException();
    }

    public void ArchiveStream(Guid streamId)
    {
        throw new NotImplementedException();
    }

    public void ArchiveStream(string streamKey)
    {
        throw new NotImplementedException();
    }

    public Task<IEventStream<T>> FetchForWriting<T>(Guid id, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(Guid id, Action<IEventStream<T>> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(Guid id, Func<IEventStream<T>, Task> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(string id, Action<IEventStream<T>> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(string id, Func<IEventStream<T>, Task> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IEventStream<T>> FetchForWriting<T>(string key, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IEventStream<T>> FetchForWriting<T>(Guid id, long expectedVersion, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IEventStream<T>> FetchForWriting<T>(string key, long expectedVersion, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(Guid id, int expectedVersion, Action<IEventStream<T>> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(Guid id, int expectedVersion, Func<IEventStream<T>, Task> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(string id, int expectedVersion, Action<IEventStream<T>> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteToAggregate<T>(string id, int expectedVersion, Func<IEventStream<T>, Task> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IEventStream<T>> FetchForExclusiveWriting<T>(Guid id, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task<IEventStream<T>> FetchForExclusiveWriting<T>(string key, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteExclusivelyToAggregate<T>(Guid id, Action<IEventStream<T>> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteExclusivelyToAggregate<T>(string id, Action<IEventStream<T>> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteExclusivelyToAggregate<T>(Guid id, Func<IEventStream<T>, Task> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Task WriteExclusivelyToAggregate<T>(string id, Func<IEventStream<T>, Task> writing, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public void OverwriteEvent(IEvent e)
    {
        throw new NotImplementedException();
    }

    public ValueTask<T?> FetchLatest<T>(Guid id, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public ValueTask<T?> FetchLatest<T>(string id, CancellationToken cancellation = new CancellationToken()) where T : class
    {
        throw new NotImplementedException();
    }

    public Guid CompletelyReplaceEvent<T>(long sequence, T eventBody) where T : class
    {
        throw new NotImplementedException();
    }
}
