namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice;
    using BackOffice.Extensions;
    using Hosts.Infrastructure.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Sync.OrganizationRegistry;

    public static class OrganizationConsumerModule
    {
        public static IServiceCollection AddOrganizationConsumerServices(this IServiceCollection services)
        {
            return services
                .RegisterOptions<OrganizationConsumerOptions>()
                .AddSingleton<IOrganizationReader, OrganizationReader>()
                .AddTraceDbConnection<OrganizationConsumerContext>(WellKnownConnectionNames.OrganizationConsumerProjections)
                .AddDbContext<OrganizationConsumerContext>(OrganizationConsumerContext.ConfigureOptions)
                .AddDbContextFactory<OrganizationConsumerContext>(OrganizationConsumerContext.ConfigureOptions)
                ;
        }
    }
}
