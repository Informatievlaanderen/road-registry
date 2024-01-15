namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Hosts.Infrastructure.Extensions;
    using Microsoft.Extensions.DependencyInjection;
    using Sync.StreetNameRegistry;

    public static class StreetNameProjectionModule
    {
        public static IServiceCollection AddStreetNameProjectionServices(this IServiceCollection services)
        {
            return services
                .AddTraceDbConnection<StreetNameProjectionContext>(WellKnownConnectionNames.StreetNameProjections, ServiceLifetime.Scoped)
                .AddDbContext<StreetNameProjectionContext>(StreetNameProjectionContext.ConfigureOptions)
                .AddDbContextFactory<StreetNameProjectionContext>(StreetNameProjectionContext.ConfigureOptions)
                .AddDbContextEventProcessorServices<StreetNameProjectionContextEventProcessor, StreetNameProjectionContext>(sp => new ConnectedProjection<StreetNameProjectionContext>[]
                {
                    new StreetNameProjection()
                })
                ;
        }
    }
}
