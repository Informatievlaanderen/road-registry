namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice;
    using Hosts.Infrastructure.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Sync.StreetNameRegistry;

    public static class StreetNameConsumerModule
    {
        public static IServiceCollection AddStreetNameConsumerServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IStreetNameEventWriter, StreetNameEventWriter>()
                .AddSingleton<IStreetNameTopicConsumer, StreetNameTopicConsumer>()
                .AddTraceDbConnection<StreetNameConsumerContext>(WellKnownConnectionNames.StreetNameConsumer)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameConsumerContext>>(StreetNameConsumerContext.ConfigureOptions)
                .AddDbContext<StreetNameConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameConsumerContext>>()(sp, options))
                .AddDbContextFactory<StreetNameConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameConsumerContext>>()(sp, options))
                ;
        }
    }
}
