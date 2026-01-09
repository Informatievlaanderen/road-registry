namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events.Projections;
using Marten;

public static class RoadNetworkChangesProjectionExtensions
{
    public static StoreOptions AddRoadNetworkChangesProjection<T>(this StoreOptions options, T projection)
        where T : RoadNetworkChangesProjection
    {
        projection.Configure(options);

        options.Projections.Add(projection,
            ProjectionLifecycle.Async,
            asyncConfiguration: opts => { opts.BatchSize = 1; });

        return options;
    }
}
