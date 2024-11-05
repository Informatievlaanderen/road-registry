namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice;
    using Microsoft.Extensions.DependencyInjection;
    using Municipality;
    using Sync.MunicipalityRegistry;

    public static class MunicipalityConsumerModule
    {
        public static IServiceCollection AddMunicipalityConsumerServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMunicipalityEventTopicConsumer, MunicipalityEventTopicConsumer>()
                .AddSingleton<ConfigureDbContextOptionsBuilder<MunicipalityEventConsumerContext>>(MunicipalityEventConsumerContext.ConfigureOptions)
                .AddDbContext<MunicipalityEventConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MunicipalityEventConsumerContext>>()(sp, options))
                .AddDbContextFactory<MunicipalityEventConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MunicipalityEventConsumerContext>>()(sp, options))
                ;
        }
    }
}
