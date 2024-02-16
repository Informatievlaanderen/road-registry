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
                        return new StreetNameSnapshotTopicConsumerByFile(sp.GetRequiredService<IDbContextFactory<StreetNameSnapshotConsumerContext>>(), jsonPath, sp.GetRequiredService<ILogger<StreetNameSnapshotTopicConsumerByFile>>());
                    }

                    return sp.GetRequiredService<StreetNameSnapshotTopicConsumer>();
                })
                .AddTraceDbConnection<StreetNameSnapshotConsumerContext>(WellKnownConnectionNames.StreetNameSnapshotConsumer)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>(StreetNameSnapshotConsumerContext.ConfigureOptions)
                .AddDbContext<StreetNameSnapshotConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>()(sp, options))
                .AddDbContextFactory<StreetNameSnapshotConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameSnapshotConsumerContext>>()(sp, options))

                .AddSingleton<StreetNameEventTopicConsumer>()
                .AddSingleton<IStreetNameEventTopicConsumer>(sp =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    var jsonPath = configuration.GetValue<string>($"Kafka:{nameof(KafkaOptions.Consumers)}:{nameof(KafkaOptions.Consumers.StreetNameEvent)}:JsonPath");
                    if (!string.IsNullOrEmpty(jsonPath))
                    {
                        // for local testing purposes
                        return new StreetNameEventTopicConsumerByFile(sp.GetRequiredService<IDbContextFactory<StreetNameEventConsumerContext>>(), jsonPath, sp.GetRequiredService<ILogger<StreetNameEventTopicConsumerByFile>>());
                    }

                    return sp.GetRequiredService<StreetNameEventTopicConsumer>();
                })
                .AddTraceDbConnection<StreetNameEventConsumerContext>(WellKnownConnectionNames.StreetNameEventConsumer)
                .AddSingleton<ConfigureDbContextOptionsBuilder<StreetNameEventConsumerContext>>(StreetNameEventConsumerContext.ConfigureOptions)
                .AddDbContext<StreetNameEventConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameEventConsumerContext>>()(sp, options))
                .AddDbContextFactory<StreetNameEventConsumerContext>((sp, options) => sp.GetRequiredService<ConfigureDbContextOptionsBuilder<StreetNameEventConsumerContext>>()(sp, options))
                ;
        }
    }
}
