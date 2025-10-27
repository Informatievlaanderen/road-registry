namespace RoadRegistry.Infrastructure.MartenDb.Setup;

using Be.Vlaanderen.Basisregisters.EventHandling;
using GradeSeparatedJunction;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Marten.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Npgsql;
using Projections;
using RoadNetwork;
using RoadNode;
using RoadSegment;
using Store;
using Weasel.Core;

public static class SetupExtensions
{
    public static IServiceCollection AddMartenRoadNetworkRepository(this IServiceCollection services)
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
                options.DatabaseSchemaName = "road";

                options.UseNewtonsoftForSerialization(enumStorage: EnumStorage.AsString, casing: Casing.CamelCase, nonPublicMembersStorage: NonPublicMembersStorage.All,
                    configure: jsonSerializerSettings =>
                    {
                        //EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                    });

                options.ConfigureRoadNetworkRepository();
                return options;
            });

        services.AddScoped<IRoadNetworkRepository, RoadNetworkRepository>();

        return services;
    }

    private static void ConfigureRoadNetworkRepository(this StoreOptions options)
    {
        options.Events.MetadataConfig.CausationIdEnabled = true;

        options.Projections.Add<RoadNetworkTopologyProjection>(ProjectionLifecycle.Inline);

        // options.Schema.For<RoadSegment>().Metadata(opts =>
        // {
        //     //opts.Version.MapTo(x => x.Version);
        // });
        options.Projections.Snapshot<RoadSegment>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<RoadNode>(SnapshotLifecycle.Inline);
        options.Projections.Snapshot<GradeSeparatedJunction>(SnapshotLifecycle.Inline);
    }
}
