namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections.Setup;
using RoadRegistry.Tests;

/// <summary>
/// Drives one or more read projections against an in-memory document store, mirroring how
/// <c>RoadNetworkChangesReadProjection</c> runs the projections in order over each event batch.
/// No Postgres/Marten infrastructure is required.
/// </summary>
public sealed class ReadProjectionScenario
{
    private readonly IReadOnlyList<IRoadNetworkChangesProjection> _projections;
    private long _position;

    public ReadProjectionScenario(params IRoadNetworkChangesProjection[] projections)
    {
        _projections = projections;
        Store = new InMemoryDocumentStoreSession(BuildStoreOptions());
    }

    public InMemoryDocumentStoreSession Store { get; }

    /// <summary>
    /// Projects the given messages (one batch) through every projection in order.
    /// Call multiple times to simulate separate correlation batches.
    /// </summary>
    public async Task GivenAsync(params object[] messages)
    {
        var events = messages.Select(BuildEvent).ToList();

        foreach (var projection in _projections)
        {
            await projection.Project(Store, events, CancellationToken.None);
        }
    }

    public Task<T?> Load<T>(object id) where T : notnull => Store.LoadAsync<T>(id);

    private IEvent BuildEvent(object message)
    {
        var eventType = typeof(Event<>).MakeGenericType(message.GetType());
        var evt = (IEvent)Activator.CreateInstance(eventType, message)!;
        evt.Version = _position;
        _position++;
        return evt;
    }

    private static StoreOptions BuildStoreOptions()
    {
        var options = new StoreOptions();
        options.ConfigureRoad();
        options.ConfigureReadDocuments();
        return options;
    }
}
