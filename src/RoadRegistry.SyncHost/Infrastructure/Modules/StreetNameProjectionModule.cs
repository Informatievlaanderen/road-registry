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
                .AddTraceDbConnection<StreetNameSnapshotProjectionContext>(WellKnownConnectionNames.StreetNameProjections)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameSnapshotProjectionContext>>(StreetNameSnapshotProjectionContext.ConfigureOptions)
                .AddDbContext<StreetNameSnapshotProjectionContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotProjectionContext>>()(sp, options))
                .AddDbContextFactory<StreetNameSnapshotProjectionContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotProjectionContext>>()(sp, options))
                .AddDbContextEventProcessorServices<StreetNameSnapshotProjectionContextEventProcessor, StreetNameSnapshotProjectionContext>(sp => new ConnectedProjection<StreetNameSnapshotProjectionContext>[]
                {
                    new StreetNameSnapshotProjection()
                })

                .AddTraceDbConnection<StreetNameEventProjectionContext>(WellKnownConnectionNames.StreetNameProjections)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameEventProjectionContext>>(StreetNameEventProjectionContext.ConfigureOptions)
                .AddDbContext<StreetNameEventProjectionContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameEventProjectionContext>>()(sp, options))
                .AddDbContextFactory<StreetNameEventProjectionContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameEventProjectionContext>>()(sp, options))
                .AddDbContextEventProcessorServices<StreetNameEventProjectionContextEventProcessor, StreetNameEventProjectionContext>(sp => new ConnectedProjection<StreetNameEventProjectionContext>[]
                {
                    new StreetNameEventProjection()
                })
                ;
        }
    }
}
