namespace RoadRegistry.Projections.IntegrationTests.Infrastructure;

using Marten;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadNode;
using RoadNode.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadSegment;
using RoadSegment.Events.V2;
using ScopedRoadNetwork;
using ScopedRoadNetwork.Events.V2;
using ScopedRoadNetwork.ValueObjects;

public class DummyRoadNetworkChangesProjection : MartenBackedRoadNetworkChangesProjection
{
    public DummyRoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection<IDocumentOperations>> projections)
        : base(projections, NullLoggerFactory.Instance)
    {
    }
}


public static class MartenProjectionIntegrationTestRunnerExtensions
{
    public static MartenProjectionIntegrationTestRunner ConfigureRoadNetworkChangesProjection<TProjection>(
        this MartenProjectionIntegrationTestRunner runner,
        Action<StoreOptions>? configureProjection = null)
        where TProjection : IRoadNetworkChangesProjection<IDocumentOperations>, new()
    {
        return ConfigureRoadNetworkChangesProjection(runner, [new TProjection()], configureProjection);
    }
    public static MartenProjectionIntegrationTestRunner ConfigureRoadNetworkChangesProjection(
        this MartenProjectionIntegrationTestRunner runner,
        IReadOnlyCollection<IRoadNetworkChangesProjection<IDocumentOperations>> projections,
        Action<StoreOptions>? configureProjection = null,
        ILogger? logger = null)
    {
        return runner.ConfigureMarten(options =>
        {
            configureProjection?.Invoke(options);

            options.AddRoadNetworkChangesProjection(new DummyRoadNetworkChangesProjection(projections));
        });
    }

    // Registers an already-built driver directly (e.g. a DbContext-backed RoadNetworkChangesProjection that owns its own
    // sub-projections and IDbContextFactory).
    public static MartenProjectionIntegrationTestRunner ConfigureRoadNetworkChangesProjection(
        this MartenProjectionIntegrationTestRunner runner,
        RoadNetworkChangesProjection projection,
        Action<StoreOptions>? configureProjection = null)
    {
        return runner.ConfigureMarten(options =>
        {
            configureProjection?.Invoke(options);

            options.AddRoadNetworkChangesProjection(projection);
        });
    }

    public static MartenProjectionIntegrationTestRunner Given<TEntity, TIdentifier>(this MartenProjectionIntegrationTestRunner runner, ICollection<(TIdentifier Identifier, object[] Events)> events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        foreach (var evt in events)
        {
            runner.Given<TEntity, TIdentifier>(evt.Identifier, evt.Events);
        }
        return runner;
    }

    public static MartenProjectionIntegrationTestRunner Given<TEntity, TIdentifier>(this MartenProjectionIntegrationTestRunner runner, Func<object, TIdentifier> getIdentifier, params object[] events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        foreach (var evt in events)
        {
            runner.Given<TEntity, TIdentifier>(getIdentifier(evt), evt);
        }
        return runner;
    }

    public static MartenProjectionIntegrationTestRunner Given<TEntity, TIdentifier>(this MartenProjectionIntegrationTestRunner runner, TIdentifier identifier, params object[] events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        return runner.Given(StreamKeyFactory.Create(typeof(TEntity), identifier), events);
    }

    public static MartenProjectionIntegrationTestRunner Given(this MartenProjectionIntegrationTestRunner runner, params object[] events)
    {
        foreach (var evt in events)
        {
            switch (evt)
            {
                case RoadNetworkWasChangedBecauseOfExtract @event:
                    runner.Given<ScopedRoadNetwork, ScopedRoadNetworkId>(new ScopedRoadNetworkId(Guid.NewGuid()), @event);
                    break;
                case MunicipalityWasMigrated @event:
                    runner.Given<ScopedRoadNetwork, ScopedRoadNetworkId>(new ScopedRoadNetworkId(Guid.NewGuid()), @event);
                    break;
                case RoadNodeWasAdded @event:
                    runner.Given<RoadNode, RoadNodeId>(@event.RoadNodeId, @event);
                    break;
                case RoadSegmentWasAdded @event:
                    runner.Given<RoadSegment, RoadSegmentId>(@event.RoadSegmentId, @event);
                    break;
                case RoadSegmentWasModified @event:
                    runner.Given<RoadSegment, RoadSegmentId>(@event.RoadSegmentId, @event);
                    break;
                case RoadSegmentWasRemoved @event:
                    runner.Given<RoadSegment, RoadSegmentId>(@event.RoadSegmentId, @event);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled event type: {evt.GetType().Name}");
            }
        }

        return runner;
    }
}
