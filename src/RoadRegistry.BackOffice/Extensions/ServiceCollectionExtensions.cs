namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using Amazon.SimpleEmailV2;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Configuration;
using Core;
using FeatureCompare;
using FeatureToggle;
using Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NodaTime;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.EuropeanRoad;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.NationalRoad;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
using RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;
using Uploads;
using IZipArchiveFeatureCompareTranslator = RoadRegistry.Extracts.FeatureCompare.DomainV2.IZipArchiveFeatureCompareTranslator;

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

    public static IServiceCollection AddHealthCommandQueue(this IServiceCollection services)
    {
        return services
            .AddSingleton<IHealthCommandQueue, HealthCommandQueue>();
    }

    public static IServiceCollection AddOrganizationCommandQueue(this IServiceCollection services)
    {
        return services
            .AddSingleton<IOrganizationCommandQueue, OrganizationCommandQueue>();
    }

    public static IServiceCollection AddEventEnricher(this IServiceCollection services)
    {
        return services
            .AddSingleton(sp => EnrichEvent.WithTime(sp.GetRequiredService<IClock>()));
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

    public static IServiceCollection AddFeatureCompare(this IServiceCollection services)
    {
        return services
            .AddFeatureCompareV1()
            .AddFeatureCompareV2()
            .AddFeatureCompareDomainV2()
            .AddSingleton<ITransactionZoneZipArchiveReader, TransactionZoneZipArchiveReader>()
            .AddSingleton<IZipArchiveBeforeFeatureCompareValidatorFactory, ZipArchiveBeforeFeatureCompareValidatorFactory>()
            .AddSingleton<IZipArchiveFeatureCompareTranslatorFactory, ZipArchiveFeatureCompareTranslatorFactory>()
            ;
    }

    private static IServiceCollection AddFeatureCompareV1(this IServiceCollection services)
    {
        return services
            .AddSingleton<FeatureCompare.V1.Readers.TransactionZoneFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.RoadNodeFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.RoadSegmentFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.RoadSegmentLaneFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.RoadSegmentWidthFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.RoadSegmentSurfaceFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.EuropeanRoadFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.NationalRoadFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.NumberedRoadFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V1.Readers.GradeSeparatedJunctionFeatureCompareFeatureReader>()

            .AddSingleton<FeatureCompare.V1.Translators.TransactionZoneFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.RoadNodeFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.IRoadSegmentFeatureCompareStreetNameContextFactory, FeatureCompare.V1.Translators.RoadSegmentFeatureCompareStreetNameContextFactory>()
            .AddSingleton<FeatureCompare.V1.Translators.RoadSegmentFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.RoadSegmentLaneFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.RoadSegmentWidthFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.RoadSegmentSurfaceFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.EuropeanRoadFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.NationalRoadFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.NumberedRoadFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V1.Translators.GradeSeparatedJunctionFeatureCompareTranslator>()

            .AddSingleton<FeatureCompare.V1.Validation.TransactionZoneZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.RoadNodeZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.RoadSegmentZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.RoadSegmentLaneZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.RoadSegmentWidthZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.RoadSegmentSurfaceZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.EuropeanRoadZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.NationalRoadZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.NumberedRoadZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.GradeSeparatedJunctionZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V1.Validation.ZipArchiveBeforeFeatureCompareValidator>()

            .AddSingleton<FeatureCompare.V1.ZipArchiveFeatureCompareTranslator>()
            ;
    }

    private static IServiceCollection AddFeatureCompareV2(this IServiceCollection services)
    {
        return services
            .AddSingleton<FeatureCompare.V2.Readers.TransactionZoneFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.RoadNodeFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.RoadSegmentFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.RoadSegmentLaneFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.RoadSegmentWidthFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.RoadSegmentSurfaceFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.EuropeanRoadFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.NationalRoadFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.NumberedRoadFeatureCompareFeatureReader>()
            .AddSingleton<FeatureCompare.V2.Readers.GradeSeparatedJunctionFeatureCompareFeatureReader>()

            .AddSingleton<FeatureCompare.V2.Translators.TransactionZoneFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.RoadNodeFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.IRoadSegmentFeatureCompareStreetNameContextFactory, FeatureCompare.V2.Translators.RoadSegmentFeatureCompareStreetNameContextFactory>()
            .AddSingleton<FeatureCompare.V2.Translators.RoadSegmentFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.RoadSegmentLaneFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.RoadSegmentWidthFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.RoadSegmentSurfaceFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.EuropeanRoadFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.NationalRoadFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.NumberedRoadFeatureCompareTranslator>()
            .AddSingleton<FeatureCompare.V2.Translators.GradeSeparatedJunctionFeatureCompareTranslator>()

            .AddSingleton<FeatureCompare.V2.Validation.TransactionZoneZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.RoadNodeZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.RoadSegmentZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.RoadSegmentLaneZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.RoadSegmentWidthZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.RoadSegmentSurfaceZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.EuropeanRoadZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.NationalRoadZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.NumberedRoadZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.GradeSeparatedJunctionZipArchiveValidator>()
            .AddSingleton<FeatureCompare.V2.Validation.ZipArchiveBeforeFeatureCompareValidator>()

            .AddSingleton<FeatureCompare.V2.ZipArchiveFeatureCompareTranslator>()
            ;
    }

    private static IServiceCollection AddFeatureCompareDomainV2(this IServiceCollection services)
    {
        return services
            .AddSingleton<IGrbOgcApiFeaturesDownloader>(_ => new GrbOgcApiFeaturesDownloader(new HttpClient(), "https://geo.api.vlaanderen.be/GRB/ogc/features/v1"))

            .AddSingleton<TransactionZoneFeatureCompareFeatureReader>()
            .AddSingleton<RoadNodeFeatureCompareFeatureReader>()
            .AddSingleton<RoadSegmentFeatureCompareFeatureReader>()
            .AddSingleton<EuropeanRoadFeatureCompareFeatureReader>()
            .AddSingleton<NationalRoadFeatureCompareFeatureReader>()
            .AddSingleton<GradeSeparatedJunctionFeatureCompareFeatureReader>()

            .AddSingleton<TransactionZoneFeatureCompareTranslator>()
            .AddSingleton<RoadNodeFeatureCompareTranslator>()
            .AddSingleton<IRoadSegmentFeatureCompareStreetNameContextFactory, RoadSegmentFeatureCompareStreetNameContextFactory>()
            .AddSingleton<RoadSegmentFeatureCompareTranslator>()
            .AddSingleton<EuropeanRoadFeatureCompareTranslator>()
            .AddSingleton<NationalRoadFeatureCompareTranslator>()
            .AddSingleton<GradeSeparatedJunctionFeatureCompareTranslator>()

            .AddSingleton<IZipArchiveFeatureCompareTranslator, ZipArchiveFeatureCompareTranslator>()
            ;
    }
}
