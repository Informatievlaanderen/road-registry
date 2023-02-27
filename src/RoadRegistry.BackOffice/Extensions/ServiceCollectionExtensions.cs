namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Core;
using FeatureToggle;
using FeatureToggles;
using Framework;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IO;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureToggles<TFeatureToggle>(this IServiceCollection services, IConfiguration configuration)
        where TFeatureToggle : IFeatureToggle
    {
        var featureTogglesConfiguration = configuration.GetSection("FeatureToggles");

        var applicationFeatureToggleType = typeof(TFeatureToggle);
        var featureToggles = applicationFeatureToggleType.Assembly
            .GetTypes()
            .Where(type => !type.IsAbstract && applicationFeatureToggleType.IsAssignableFrom(type))
            .Select(type =>
            {
                var configurationKey = type.Name;
                if (configurationKey.EndsWith("FeatureToggle"))
                {
                    configurationKey = configurationKey.Substring(0, configurationKey.Length - "FeatureToggle".Length);
                }

                var featureEnabled = featureTogglesConfiguration.GetValue<bool>(configurationKey);
                var featureToggle = Activator.CreateInstance(type, new object[] { featureEnabled });
                return featureToggle;
            })
            .ToList();

        foreach (var featureToggle in featureToggles)
        {
            services.AddSingleton(featureToggle.GetType(), featureToggle);
        }

        return services;
    }

    public static IServiceCollection AddRoadNetworkCommandQueue(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRoadNetworkCommandQueue, RoadNetworkCommandQueue>();
    }

    public static IServiceCollection AddCommandHandlerDispatcher(this IServiceCollection services, Func<IServiceProvider, CommandHandlerResolver> commandHandlerResolverBuilder)
    {
        return services
                .AddSingleton(sp => Dispatch.Using(commandHandlerResolverBuilder(sp), sp.GetRequiredService<ApplicationMetadata>()));
    }

    public static IServiceCollection AddDistributedS3Cache(this IServiceCollection services)
    {
        return services
            .RegisterOptions<DistributedS3CacheOptions>(nameof(DistributedS3CacheOptions))
            .AddSingleton<DistributedS3Cache>()
            .AddTransient<S3CacheService>();
    }

    public static IServiceCollection AddRoadRegistrySnapshot(this IServiceCollection services) => services
        .AddDistributedS3Cache()
        .AddSingleton(new RecyclableMemoryStreamManager())
        .AddSingleton(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return new RoadNetworkSnapshotsBlobClient(
                new SqlBlobClient(
                    new SqlConnectionStringBuilder(configuration.GetConnectionString(WellknownConnectionNames.Snapshots)),
                    WellknownSchemas.SnapshotSchema));
        })
        .AddSingleton<IRoadNetworkSnapshotReader>(sp =>
        {
            var featureToggle = sp.GetRequiredService<UseSnapshotSqsRequestFeatureToggle>();
            return featureToggle.FeatureEnabled
                ? new RoadNetworkSnapshotReader(sp.GetRequiredService<S3CacheService>())
                : new RoadNetworkSnapshotReaderWriter(sp.GetRequiredService<RoadNetworkSnapshotsBlobClient>(), sp.GetRequiredService<RecyclableMemoryStreamManager>());
        })
        .AddSingleton<IRoadNetworkSnapshotWriter>(sp =>
        {
            var featureToggle = sp.GetRequiredService<UseSnapshotSqsRequestFeatureToggle>();
            return featureToggle.FeatureEnabled
                ? new RoadNetworkSnapshotWriter(sp.GetRequiredService<S3CacheService>())
                : new RoadNetworkSnapshotReaderWriter(sp.GetRequiredService<RoadNetworkSnapshotsBlobClient>(), sp.GetRequiredService<RecyclableMemoryStreamManager>());
        });

    public static IServiceCollection RegisterOptions<TOptions>(this IServiceCollection services)
        where TOptions : class, new()
    {
        return services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetOptions<TOptions>());
    }

    public static IServiceCollection RegisterOptions<TOptions>(this IServiceCollection services, string configurationSectionKey)
        where TOptions : class, new()
    {
        return services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetOptions<TOptions>(configurationSectionKey));
    }
}
