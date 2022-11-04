namespace RoadRegistry.StreetNameConsumer.ProjectionHost;

using Autofac;
using Be.Vlaanderen.Basisregisters.Projector;
using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
using Extensions;
using Hosts;
using Projections;
using Schema;

public class ConsumerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterDbContext<StreetNameConsumerContext>(WellknownConnectionNames.StreetNameConsumer,
            sqlServerOptions => sqlServerOptions
                .EnableRetryOnFailure()
                .MigrationsHistoryTable(MigrationTables.StreetNameConsumer, WellknownSchemas.StreetNameConsumerSchema)
            , dbContextOptionsBuilder =>
                new StreetNameConsumerContext(dbContextOptionsBuilder.Options));

        builder
            .RegisterProjectionMigrator<StreetNameConsumerContextMigrationFactory>()
            .RegisterProjections<StreetNameConsumerProjection, StreetNameConsumerContext>(
                context => new StreetNameConsumerProjection(),
                ConnectedProjectionSettings.Default);

    }
}
