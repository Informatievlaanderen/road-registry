namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using GradeSeparatedJunction;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Npgsql;
using Projections;
using RoadNetwork;
using RoadNode;
using RoadSegment;
using Store;
using Weasel.Core;

public static class SetupExtensions
{
    public static IServiceCollection AddMartenRoad(this IServiceCollection services, Action<StoreOptions>? configure = null)
    {
        services
            .AddMarten(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("Marten");

                var options = new StoreOptions();
                options.Connection(new NpgsqlDataSourceBuilder(connectionString)
                    .UseNetTopologySuite()
                    .Build());
                options.ConfigureRoad();
                configure?.Invoke(options);

                return options;
            });

        services.AddSingleton<IRoadNetworkRepository, RoadNetworkRepository>();

        return services;
    }

    public static void ConfigureRoad(this StoreOptions options)
    {
        //TODO-pr update schemas: `eventstore` for event related, `projections` for all projections
        options.DatabaseSchemaName = "road";

        options.ConfigureSerializer();

        options.Events.StreamIdentity = StreamIdentity.AsString;
        options.Events.MetadataConfig.CausationIdEnabled = true;
        options.Events.MetadataConfig.CorrelationIdEnabled = true;
        options.Events.MetadataConfig.HeadersEnabled = true;
    }

    public static StoreOptions ConfigureSerializer(this StoreOptions options)
    {
        options.UseNewtonsoftForSerialization(
            enumStorage: EnumStorage.AsString,
            casing: Casing.CamelCase,
            nonPublicMembersStorage: NonPublicMembersStorage.All,
            configure: settings =>
            {
                settings.MaxDepth = 32;
                settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                settings
                    .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                    .WithIsoIntervalConverter();

                // Do not change this setting
                // Setting this to None prevents Json.NET from loading malicious, unsafe, or security-sensitive types
                settings.TypeNameHandling = TypeNameHandling.None;

                foreach (var converter in WellKnownJsonConverters.Converters)
                {
                    settings.Converters.Add(converter);
                }

                settings.NullValueHandling = NullValueHandling.Include;
                settings.Formatting = Formatting.None;
            });
        return options;
    }

    public static StoreOptions AddRoadNetworkTopologyProjection(this StoreOptions options)
    {
        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline);

        options.Projections.Snapshot<RoadSegment>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<RoadNode>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeSeparatedJunction>(SnapshotLifecycle.Inline);

        return options;
    }
}
