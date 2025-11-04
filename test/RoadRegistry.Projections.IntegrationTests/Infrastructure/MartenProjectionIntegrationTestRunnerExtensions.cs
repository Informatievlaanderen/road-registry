namespace RoadRegistry.Projections.IntegrationTests.Infrastructure;

using BackOffice;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;

public static class MartenProjectionIntegrationTestRunnerExtensions
{
    public static MartenProjectionIntegrationTestRunner ConfigureRoadNetworkChangesProjection<TProjection>(
        this MartenProjectionIntegrationTestRunner runner,
        Action<StoreOptions>? configureProjection = null)
        where TProjection : IRoadNetworkChangesProjection, new()
    {
        return ConfigureRoadNetworkChangesProjection(runner, [new TProjection()], configureProjection);
    }
    public static MartenProjectionIntegrationTestRunner ConfigureRoadNetworkChangesProjection(
        this MartenProjectionIntegrationTestRunner runner,
        IReadOnlyCollection<IRoadNetworkChangesProjection> projections,
        Action<StoreOptions>? configureProjection = null)
    {
        return runner.ConfigureMarten(options =>
        {
            configureProjection?.Invoke(options);

            options.AddRoadNetworkChangesProjection(
                "projection_roadnetworkchanges",
                projections);
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
                case RoadNodeAdded @event:
                    runner.Given<RoadRegistry.RoadNode.RoadNode, RoadNodeId>(@event.Id, @event);
                    break;
                case RoadSegmentAdded @event:
                    runner.Given<RoadRegistry.RoadSegment.RoadSegment, RoadSegmentId>(@event.Id, @event);
                    break;
                case RoadSegmentModified @event:
                    runner.Given<RoadRegistry.RoadSegment.RoadSegment, RoadSegmentId>(@event.Id, @event);
                    break;
                case RoadSegmentRemoved @event:
                    runner.Given<RoadRegistry.RoadSegment.RoadSegment, RoadSegmentId>(@event.Id, @event);
                    break;
                default:
                    throw new NotImplementedException($"Unhandled event type: {evt.GetType().Name}");
            }
        }

        return runner;
    }
}
