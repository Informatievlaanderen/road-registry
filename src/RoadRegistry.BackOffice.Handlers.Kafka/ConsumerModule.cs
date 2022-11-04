namespace RoadRegistry.BackOffice.Handlers.Kafka;

using Autofac;
using Extensions;
using Hosts;
using RoadRegistry.StreetNameConsumer.Schema;

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
    }
}
