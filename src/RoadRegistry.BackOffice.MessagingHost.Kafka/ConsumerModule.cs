namespace RoadRegistry.BackOffice.MessagingHost.Kafka
{
    using Autofac;
    using Extensions.Autofac;
    using RoadRegistry.Hosts;

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
}
