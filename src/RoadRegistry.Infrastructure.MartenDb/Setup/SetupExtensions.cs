namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using BackOffice;
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

        services.AddScoped<IRoadNetworkRepository, RoadNetworkRepository>();

        return services;
    }

    public static void ConfigureRoad(this StoreOptions options)
    {
        options.DatabaseSchemaName = "road";

        options.UseNewtonsoftForSerialization(
            enumStorage: EnumStorage.AsString,
            casing: Casing.CamelCase,
            nonPublicMembersStorage: NonPublicMembersStorage.All,
            configure: jsonSerializerSettings =>
            {
                jsonSerializerSettings.MaxDepth = 32;
                jsonSerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                jsonSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                jsonSerializerSettings
                    .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb)
                    .WithIsoIntervalConverter();

                foreach (var converter in WellKnownJsonConverters.Converters)
                {
                    jsonSerializerSettings.Converters.Add(converter);
                }
            });

        options.Events.StreamIdentity = StreamIdentity.AsString;
        options.Events.MetadataConfig.CausationIdEnabled = true;
        options.Events.MetadataConfig.CorrelationIdEnabled = true;
        options.Events.MetadataConfig.HeadersEnabled = true;
    }

    public static void AddRoadNetworkTopologyProjection(this StoreOptions options)
    {
        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline);

        options.Projections.Snapshot<RoadSegment>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<RoadNode>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeSeparatedJunction>(SnapshotLifecycle.Inline);
    }
}
