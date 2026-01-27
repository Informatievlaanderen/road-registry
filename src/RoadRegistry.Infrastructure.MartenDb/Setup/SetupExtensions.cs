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
using Newtonsoft.Json.Serialization;
using NodaTime;
using NodaTime.Serialization.JsonNet;
using Npgsql;
using Projections;
using RoadNode;
using RoadSegment;
using ScopedRoadNetwork;
using Store;
using Weasel.Core;

public static class SetupExtensions
{
    public static IServiceCollection AddMartenRoad(this IServiceCollection services, Action<StoreOptions>? configure = null)
    {
        return AddMartenRoad(services, (options, _) =>
        {
            configure?.Invoke(options);
        });
    }

    public static IServiceCollection AddMartenRoad(this IServiceCollection services, Action<StoreOptions, IServiceProvider>? configure = null)
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
                configure?.Invoke(options, sp);

                return options;
            });

        services.AddSingleton<IRoadNetworkRepository, RoadNetworkRepository>();

        return services;
    }

    public static void ConfigureRoad(this StoreOptions options)
    {
        options.DatabaseSchemaName = WellKnownSchemas.MartenEventStore;

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

                settings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        OverrideSpecifiedNames = true,
                        ProcessDictionaryKeys = false,
                        ProcessExtensionDataNames = true
                    }
                };

                settings.NullValueHandling = NullValueHandling.Include;
                settings.Formatting = Formatting.None;
            });
        return options;
    }

    public static StoreOptions AddRoadNetworkTopologyProjection(this StoreOptions options)
    {
        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline);
        return options;
    }
    public static StoreOptions AddRoadAggregatesSnapshots(this StoreOptions options)
    {
        options.Projections.Snapshot<RoadSegment>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<RoadNode>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeSeparatedJunction>(SnapshotLifecycle.Inline);

        return options;
    }
}
