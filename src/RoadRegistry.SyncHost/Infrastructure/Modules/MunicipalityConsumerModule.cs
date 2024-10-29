namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice;
    using Microsoft.Extensions.DependencyInjection;
    using Sync.MunicipalityRegistry;

    public static class MunicipalityConsumerModule
    {
        public static IServiceCollection AddMunicipalityConsumerServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMunicipalityEventWriter, MunicipalityEventWriter>()

                .AddSingleton<MunicipalityEventTopicConsumer>()
                .AddSingleton<IMunicipalityEventTopicConsumer>(sp =>
                {
                    //var options = sp.GetRequiredService<KafkaOptions>();
                    //var jsonPath = options.Consumers.MunicipalityEvent.JsonPath;
                    // if (!string.IsNullOrEmpty(jsonPath))
                    // {
                    //     // for local testing purposes
                    //     return new MunicipalityEventTopicConsumerByFile(sp.GetRequiredService<IDbContextFactory<MunicipalityEventConsumerContext>>(), jsonPath, sp.GetRequiredService<ILogger<MunicipalityEventTopicConsumerByFile>>());
                    // }

                    return sp.GetRequiredService<MunicipalityEventTopicConsumer>();
                })
                .AddSingleton<ConfigureDbContextOptionsBuilder<MunicipalityEventConsumerContext>>(MunicipalityEventConsumerContext.ConfigureOptions)
                .AddDbContext<MunicipalityEventConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MunicipalityEventConsumerContext>>()(sp, options))
                .AddDbContextFactory<MunicipalityEventConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<MunicipalityEventConsumerContext>>()(sp, options))
                ;
        }
    }
}
