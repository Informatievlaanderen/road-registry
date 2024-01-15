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
                .AddTraceDbConnection<StreetNameConsumerContext>(WellKnownConnectionNames.StreetNameConsumer, ServiceLifetime.Scoped)
                .AddDbContext<StreetNameConsumerContext>(StreetNameConsumerContext.ConfigureOptions)
                .AddDbContextFactory<StreetNameConsumerContext>(StreetNameConsumerContext.ConfigureOptions)
                ;
        }
    }
}
