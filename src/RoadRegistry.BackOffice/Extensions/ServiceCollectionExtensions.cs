namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Configuration;
using System.Linq;
using Amazon.SimpleEmailV2;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Core;
using FeatureToggle;
using FeatureToggles;
using Framework;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NodaTime;
using SqlStreamStore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailClient(this IServiceCollection services, IConfiguration configuration)
    {
        var emailClientOptions = configuration.GetOptions<EmailClientOptions>() ?? new EmailClientOptions();

        services.AddSingleton(emailClientOptions);
        services.AddSingleton(_ => new AmazonSimpleEmailServiceV2Client());
        services.AddSingleton<IExtractUploadFailedEmailClient>(sp => !string.IsNullOrEmpty(emailClientOptions.FromEmailAddress)
            ? new ExtractUploadFailedEmailClient(
                sp.GetService<AmazonSimpleEmailServiceV2Client>(),
                sp.GetService<EmailClientOptions>(),
                sp.GetService<ILogger<ExtractUploadFailedEmailClient>>())
            : new NotConfiguredExtractUploadFailedEmailClient(sp.GetService<ILogger<NotConfiguredExtractUploadFailedEmailClient>>()));

        return services;
    }

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

    public static IServiceCollection AddRoadNetworkEventWriter(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRoadNetworkEventWriter>(sp => new RoadNetworkEventWriter(sp.GetRequiredService<IStreamStore>(), EnrichEvent.WithTime(sp.GetRequiredService<IClock>())));
    }

    public static IServiceCollection AddDistributedS3Cache(this IServiceCollection services)
    {
        return services
            .RegisterOptions<DistributedS3CacheOptions>(nameof(DistributedS3CacheOptions), options =>
            {
                if (string.IsNullOrEmpty(options.Bucket))
                {
                    throw new ConfigurationErrorsException($"'{nameof(options.Bucket)}' is required");
                }
                if (string.IsNullOrEmpty(options.RootDir))
                {
                    throw new ConfigurationErrorsException($"'{nameof(options.RootDir)}' is required");
                }
            })
            .AddSingleton<DistributedS3Cache>()
            .AddTransient<S3CacheService>();
    }

    public static IServiceCollection AddRoadRegistrySnapshot(this IServiceCollection services)
    {
        return services
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
    }

    public static IServiceCollection RegisterOptions<TOptions>(this IServiceCollection services, Action<TOptions> validateOptions = null)
        where TOptions : class, new()
    {
        if (validateOptions != null)
        {
            services.AddOptionsValidator(validateOptions);
        }

        return services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetOptions<TOptions>());
    }

    public static IServiceCollection RegisterOptions<TOptions>(this IServiceCollection services, string configurationSectionKey, Action<TOptions> validateOptions = null)
        where TOptions : class, new()
    {
        if (validateOptions != null)
        {
            services.AddOptionsValidator(validateOptions);
        }

        return services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetOptions<TOptions>(configurationSectionKey));
    }

    public static IServiceCollection AddRoadNetworkSnapshotStrategyOptions(this IServiceCollection services)
    {
        return services
            .RegisterOptions<RoadNetworkSnapshotStrategyOptions>(options =>
            {
                if (options.EventCount <= 0)
                {
                    throw new ConfigurationErrorsException($"{nameof(options.EventCount)} must be greater than zero");
                }
            });
    }
}
