namespace RoadRegistry.SyncHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.Projector;
using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
using RoadRegistry.BackOffice;
using RoadRegistry.StreetNameConsumer.Schema;
using RoadRegistry.SyncHost.Extensions;
using Sync.StreetNameRegistry;

public class StreetNameConsumerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterDbContext<StreetNameConsumerContext>(WellknownConnectionNames.StreetNameConsumerProjections,
            sqlServerOptions => sqlServerOptions
                .EnableRetryOnFailure()
                .MigrationsHistoryTable(MigrationTables.StreetNameConsumer, WellknownSchemas.StreetNameConsumerSchema)
            , dbContextOptionsBuilder =>
                new StreetNameConsumerContext(dbContextOptionsBuilder.Options));

        builder
            .RegisterProjectionMigrator<StreetNameConsumerContextMigrationFactory>();
            //.RegisterProjections<StreetNameConsumerProjection, StreetNameConsumerContext>( //TODO-rik is dit nog nodig?
            //    context => new StreetNameConsumerProjection(),
            //    ConnectedProjectionSettings.Default);

    }
}
