namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Sync.OrganizationRegistry;

    public static class OrganizationConsumerModule
    {
        public static IServiceCollection AddOrganizationConsumerServices(this IServiceCollection services)
        {
            return services
                .RegisterOptions<OrganizationConsumerOptions>()
                .AddSingleton<IOrganizationReader, OrganizationReader>()
                .AddDbContext<OrganizationConsumerContext>(OrganizationConsumerContext.ConfigureOptions)
                .AddDbContextFactory<OrganizationConsumerContext>(OrganizationConsumerContext.ConfigureOptions)
                ;
        }
    }
}
