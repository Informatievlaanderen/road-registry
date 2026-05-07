namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment;
using RoadRegistry.ScopedRoadNetwork;
using Weasel.Core;

public static class SetupExtensions
{
    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMartenRoad(this IServiceCollection services, Action<StoreOptions>? configure = null)
    {
        return AddMartenRoad(services, (options, _) =>
        {
            configure?.Invoke(options);
        });
    }

    public static MartenServiceCollectionExtensions.MartenConfigurationExpression AddMartenRoad(this IServiceCollection services, Action<StoreOptions, IServiceProvider>? configure = null)
    {
        services.AddSingleton<IRoadNetworkRepository, RoadNetworkRepository>();

        return services
            .AddMarten(sp =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetRequiredConnectionString("Marten");

                var options = new StoreOptions();
                options.Connection(new NpgsqlDataSourceBuilder(connectionString)
                    .UseNetTopologySuite()
                    .Build());
                options.ConfigureRoad();
                configure?.Invoke(options, sp);

                return options;
            });
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
                settings.ConfigureForMarten();
            });
        return options;
    }

    public static StoreOptions AddRoadNetworkTopologyProjection(this StoreOptions options)
    {
        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline, opts => opts.BatchSize = 5000);
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
