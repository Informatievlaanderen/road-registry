namespace RoadRegistry.SyncHost;

using Autofac;
using RoadRegistry.BackOffice;
using RoadRegistry.SyncHost.Extensions;
using Sync.StreetNameRegistry;

public class StreetNameConsumerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterDbContext<StreetNameConsumerContext>(WellKnownConnectionNames.StreetNameConsumerProjections,
                sqlServerOptions => sqlServerOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetNameConsumer, WellKnownSchemas.StreetNameConsumerSchema)
                , dbContextOptionsBuilder =>
                    new StreetNameConsumerContext(dbContextOptionsBuilder.Options))
            .RegisterProjectionMigrator<StreetNameConsumerContextMigrationFactory>()

            .RegisterDbContext<StreetNameProjectionContext>(WellKnownConnectionNames.StreetNameProjections,
                sqlServerOptions => sqlServerOptions
                    .EnableRetryOnFailure()
                    .MigrationsHistoryTable(MigrationTables.StreetName, WellKnownSchemas.StreetNameSchema)
                , dbContextOptionsBuilder =>
                    new StreetNameProjectionContext(dbContextOptionsBuilder.Options))
            .RegisterProjectionMigrator<StreetNameProjectionContextMigrationFactory>()
            ;

            //.RegisterProjections<StreetNameConsumerProjection, StreetNameConsumerContext>( //TODO-rik is dit nog nodig?
            //    context => new StreetNameConsumerProjection(),
            //    ConnectedProjectionSettings.Default);
    }
}
