namespace RoadRegistry.BackOffice.Extensions;

using Amazon.SimpleEmailV2;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
using Configuration;
using Core;
using FeatureCompare;
using FeatureCompare.Readers;
using FeatureCompare.Translators;
using FeatureCompare.Validation;
using FeatureToggle;
using Framework;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Uploads;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmailClient(this IServiceCollection services)
    {
        services.AddSingleton(sp => sp.GetRequiredService<IConfiguration>().GetOptions<EmailClientOptions>() ?? new EmailClientOptions());
        services.AddSingleton(_ => new AmazonSimpleEmailServiceV2Client());
        services.AddSingleton<IExtractUploadFailedEmailClient>(sp =>
        {
            var emailClientOptions = sp.GetRequiredService<EmailClientOptions>();

            return !string.IsNullOrEmpty(emailClientOptions.FromEmailAddress)
                ? new ExtractUploadFailedEmailClient(
                    sp.GetService<AmazonSimpleEmailServiceV2Client>(),
                    sp.GetService<EmailClientOptions>(),
                    sp.GetService<ILogger<ExtractUploadFailedEmailClient>>())
                : new NotConfiguredExtractUploadFailedEmailClient(sp.GetService<ILogger<NotConfiguredExtractUploadFailedEmailClient>>());
        });

        return services;
    }

    public static ICollection<TFeatureToggle> GetFeatureToggles<TFeatureToggle>(this IConfiguration configuration)
        where TFeatureToggle : IFeatureToggle
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var featureTogglesConfiguration = configuration.GetSection("FeatureToggles");

        var applicationFeatureToggleType = typeof(TFeatureToggle);
        return applicationFeatureToggleType.Assembly
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
                var featureToggle = (TFeatureToggle)Activator.CreateInstance(type, new object[] { featureEnabled });
                return featureToggle;
            })
            .ToList();
    }

    public static IServiceCollection AddFeatureToggles<TFeatureToggle>(this IServiceCollection services, IConfiguration configuration)
        where TFeatureToggle : IFeatureToggle
    {
        return services.AddFeatureToggles(configuration.GetFeatureToggles<TFeatureToggle>());
    }

    public static IServiceCollection AddFeatureToggles<TFeatureToggle>(this IServiceCollection services, IEnumerable<TFeatureToggle> featureToggles)
        where TFeatureToggle : IFeatureToggle
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(featureToggles);

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

    public static IServiceCollection AddOrganizationCommandQueue(this IServiceCollection services)
    {
        return services
            .AddSingleton<IOrganizationCommandQueue, OrganizationCommandQueue>();
    }

    public static IServiceCollection AddEventEnricher(this IServiceCollection services)
    {
        return services
            .AddSingleton<EventEnricher>(sp => EnrichEvent.WithTime(sp.GetRequiredService<IClock>()));
    }

    public static IServiceCollection AddRoadNetworkEventWriter(this IServiceCollection services)
    {
        return services
            .AddSingleton<IRoadNetworkEventWriter, RoadNetworkEventWriter>();
    }

    public static IServiceCollection AddCommandHandlerDispatcher(this IServiceCollection services, Func<IServiceProvider, CommandHandlerResolver> commandHandlerResolverBuilder)
    {
        return services
            .AddSingleton(sp => Dispatch.Using(commandHandlerResolverBuilder(sp), sp.GetRequiredService<ApplicationMetadata>()));
    }

    public static IServiceCollection AddDistributedS3Cache(this IServiceCollection services)
    {
        return services
            .RegisterOptions<DistributedS3CacheOptions>(nameof(DistributedS3CacheOptions), options =>
            {
                if (string.IsNullOrEmpty(options.Bucket))
                {
                    throw new ConfigurationErrorsException($"'{nameof(options.Bucket)}' is required for '{options.GetType().Name}'");
                }
                if (string.IsNullOrEmpty(options.RootDir))
                {
                    throw new ConfigurationErrorsException($"'{nameof(options.RootDir)}' is required for '{options.GetType().Name}'");
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
            .AddSingleton(sp => new RoadNetworkSnapshotsBlobClient(sp.GetService<IBlobClientFactory>().Create(WellKnownBuckets.SnapshotsBucket)))
            .AddSingleton<IRoadNetworkSnapshotReader, RoadNetworkSnapshotReader>()
            .AddSingleton<IRoadNetworkSnapshotWriter, RoadNetworkSnapshotWriter>();
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

    public static IServiceCollection AddFeatureCompare(this IServiceCollection services)
    {
        return services
            .AddSingleton<ITransactionZoneFeatureCompareFeatureReader, TransactionZoneFeatureCompareFeatureReader>()
            .AddSingleton<RoadNodeFeatureCompareFeatureReader>()
            .AddSingleton<RoadSegmentFeatureCompareFeatureReader>()
            .AddSingleton<RoadSegmentLaneFeatureCompareFeatureReader>()
            .AddSingleton<RoadSegmentWidthFeatureCompareFeatureReader>()
            .AddSingleton<RoadSegmentSurfaceFeatureCompareFeatureReader>()
            .AddSingleton<EuropeanRoadFeatureCompareFeatureReader>()
            .AddSingleton<NationalRoadFeatureCompareFeatureReader>()
            .AddSingleton<NumberedRoadFeatureCompareFeatureReader>()
            .AddSingleton<GradeSeparatedJunctionFeatureCompareFeatureReader>()

            .AddSingleton<TransactionZoneFeatureCompareTranslator>()
            .AddSingleton<RoadNodeFeatureCompareTranslator>()
            .AddSingleton<IRoadSegmentFeatureCompareStreetNameContextFactory, RoadSegmentFeatureCompareStreetNameContextFactory>()
            .AddSingleton<RoadSegmentFeatureCompareTranslator>()
            .AddSingleton<RoadSegmentLaneFeatureCompareTranslator>()
            .AddSingleton<RoadSegmentWidthFeatureCompareTranslator>()
            .AddSingleton<RoadSegmentSurfaceFeatureCompareTranslator>()
            .AddSingleton<EuropeanRoadFeatureCompareTranslator>()
            .AddSingleton<NationalRoadFeatureCompareTranslator>()
            .AddSingleton<NumberedRoadFeatureCompareTranslator>()
            .AddSingleton<GradeSeparatedJunctionFeatureCompareTranslator>()
            .AddSingleton<IZipArchiveBeforeFeatureCompareValidator, ZipArchiveBeforeFeatureCompareValidator>()

            .AddSingleton<TransactionZoneZipArchiveValidator>()
            .AddSingleton<RoadNodeZipArchiveValidator>()
            .AddSingleton<RoadSegmentZipArchiveValidator>()
            .AddSingleton<RoadSegmentLaneZipArchiveValidator>()
            .AddSingleton<RoadSegmentWidthZipArchiveValidator>()
            .AddSingleton<RoadSegmentSurfaceZipArchiveValidator>()
            .AddSingleton<EuropeanRoadZipArchiveValidator>()
            .AddSingleton<NationalRoadZipArchiveValidator>()
            .AddSingleton<NumberedRoadZipArchiveValidator>()
            .AddSingleton<GradeSeparatedJunctionZipArchiveValidator>()
            .AddSingleton<IZipArchiveFeatureCompareTranslator, ZipArchiveFeatureCompareTranslator>()
            ;
    }
}
