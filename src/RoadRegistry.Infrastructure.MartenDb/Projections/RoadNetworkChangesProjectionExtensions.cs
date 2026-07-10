namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using BackOffice;
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
            asyncConfiguration: opts =>
            {
                opts.EnableDocumentTrackingByIdentity = true;
                opts.BatchSize = projection.BatchSize;
            });

        return options;
    }

    public static StoreOptions ConfigureRoadNetworkChangesProgression(this StoreOptions options)
    {
        options.Schema.For<RoadNetworkChangesProjectionProgression>()
            .DatabaseSchemaName(WellKnownSchemas.MartenEventStore)
            .DocumentAlias("roadnetworkchangesprojection_progression")
            .Identity(x => x.Id)
            .Duplicate(x => x.ProjectionName, configure: index => { index.Name = "ix_changesprojection_projectionname"; }, notNull: true)
            .Duplicate(x => x.LastSequenceId, configure: index => { index.Name = "ix_changesprojection_lastsequenceid"; }, notNull: true)
            ;

        return options;
    }
}
