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
                .AddSingleton<IStreetNameSnapshotTopicConsumer, StreetNameSnapshotTopicConsumer>()
                .AddTraceDbConnection<StreetNameSnapshotConsumerContext>(WellKnownConnectionNames.StreetNameSnapshotConsumer)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>(StreetNameSnapshotConsumerContext.ConfigureOptions)
                .AddDbContext<StreetNameSnapshotConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>()(sp, options))
                .AddDbContextFactory<StreetNameSnapshotConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>()(sp, options))
                ;
        }
    }
}
