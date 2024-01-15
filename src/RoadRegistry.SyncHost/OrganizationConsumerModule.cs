namespace RoadRegistry.SyncHost;

using Autofac;
using BackOffice;
using Extensions;
using Sync.OrganizationRegistry;
using System.Net.Http;

public class OrganizationConsumerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<IOrganizationReader>(c =>
            new OrganizationReader(c.Resolve<IHttpClientFactory>(), c.Resolve<OrganizationConsumerOptions>())
        );

        builder.RegisterDbContext<OrganizationConsumerContext>(WellKnownConnectionNames.OrganizationConsumerProjections,
            sqlServerOptions => sqlServerOptions
                .EnableRetryOnFailure()
                .MigrationsHistoryTable(MigrationTables.OrganizationConsumer, WellKnownSchemas.OrganizationConsumerSchema)
            , dbContextOptionsBuilder =>
                new OrganizationConsumerContext(dbContextOptionsBuilder.Options)
        );

        builder
            .RegisterProjectionMigrator<OrganizationConsumerContextMigratorFactory>()
            ;
    }
}
