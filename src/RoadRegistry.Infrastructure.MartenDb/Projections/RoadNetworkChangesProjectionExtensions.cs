namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events.Projections;
using Marten;

public static class RoadNetworkChangesProjectionExtensions
{
    public static StoreOptions AddRoadNetworkChangesProjection(this StoreOptions options,
        IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        options.Projections.Add(new RoadNetworkChangeProjection(projections),
            ProjectionLifecycle.Async,
            asyncConfiguration: opts =>
            {
                opts.BatchSize = 1;
            });

        return options;
    }
}
