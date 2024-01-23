namespace RoadRegistry.SyncHost.Infrastructure.Modules
{
    using BackOffice;
    using Hosts.Infrastructure.Extensions;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Sync.StreetNameRegistry;

    public static class StreetNameConsumerModule
    {
        public static IServiceCollection AddStreetNameConsumerServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<IStreetNameEventWriter, StreetNameEventWriter>()
                .AddSingleton<StreetNameSnapshotTopicConsumer>()
                .AddSingleton<IStreetNameSnapshotTopicConsumer>(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var jsonPath = configuration.GetValue<string>($"Kafka:{nameof(KafkaOptions.Consumers)}:{nameof(KafkaOptions.Consumers.StreetNameSnapshot)}:JsonPath");
                    if (!string.IsNullOrEmpty(jsonPath))
                    {
                        // for local testing purposes
                        return new StreetNameTopicConsumerByFile(sp.GetRequiredService<IDbContextFactory<StreetNameSnapshotConsumerContext>>(), jsonPath, sp.GetRequiredService<ILogger<StreetNameTopicConsumerByFile>>());
                    }

                    return sp.GetRequiredService<StreetNameSnapshotTopicConsumer>();
                })
                .AddTraceDbConnection<StreetNameSnapshotConsumerContext>(WellKnownConnectionNames.StreetNameSnapshotConsumer)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>(StreetNameSnapshotConsumerContext.ConfigureOptions)
                .AddDbContext<StreetNameSnapshotConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>()(sp, options))
                .AddDbContextFactory<StreetNameSnapshotConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>()(sp, options))
                ;
        }
    }
}
