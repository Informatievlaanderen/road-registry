namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections;

using Marten;
using RoadNode.Events;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadSegment.Events;
using RoadSegment.ValueObjects;

public static class MartenProjectionTestRunnerExtensions
{
    public static MartenProjectionTestRunner ConfigureRoadNetworkChangesProjection<TProjection>(
        this MartenProjectionTestRunner runner,
        Action<StoreOptions>? configureProjection = null)
        where TProjection : IRoadNetworkChangesProjection, new()
    {
        return runner.ConfigureMarten(options =>
        {
            configureProjection?.Invoke(options);

            options.AddRoadNetworkChangesProjection(
                "projection_roadnetworkchanges",
                [new TProjection()]);
        });
    }

    public static MartenProjectionTestRunner ConfigureRoadNetworkChangesProjection(
        this MartenProjectionTestRunner runner,
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

    public static MartenProjectionTestRunner Given<TEntity, TIdentifier>(this MartenProjectionTestRunner runner, ICollection<(TIdentifier Identifier, object[] Events)> events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        foreach (var evt in events)
        {
            runner.Given<TEntity, TIdentifier>(evt.Identifier, evt.Events);
        }
        return runner;
    }

    public static MartenProjectionTestRunner Given<TEntity, TIdentifier>(this MartenProjectionTestRunner runner, Func<object, TIdentifier> getIdentifier, params object[] events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        foreach (var evt in events)
        {
            runner.Given<TEntity, TIdentifier>(getIdentifier(evt), evt);
        }
        return runner;
    }

    public static MartenProjectionTestRunner Given<TEntity, TIdentifier>(this MartenProjectionTestRunner runner, TIdentifier identifier, params object[] events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        return runner.Given(StreamKeyFactory.Create(typeof(TEntity), identifier), events);
    }

    public static MartenProjectionTestRunner Given(this MartenProjectionTestRunner runner, params object[] events)
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
                    throw new NotImplementedException($"Unhanled event type: {evt.GetType().Name}");
            }
        }

        return runner;
    }
}
