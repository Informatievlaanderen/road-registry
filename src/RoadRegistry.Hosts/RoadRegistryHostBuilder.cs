namespace RoadRegistry.Hosts;

using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using Serilog;
using Serilog.Debugging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;


public sealed class RoadRegistryHostBuilder<T> : HostBuilder
{
    private readonly string[] _args;
    private Func<IServiceProvider, Task> _runCommandDelegate;

    private RoadRegistryHostBuilder()
    {
        AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
            Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

        AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

        ConfigureHostConfiguration(builder => { })
            .ConfigureAppConfiguration((hostContext, builder) => { })
            .ConfigureLogging((hostContext, builder) => { });
    }

    public RoadRegistryHostBuilder(string[] args) : this()
    {
        _args = args;
    }

    public new RoadRegistryHost<T> Build()
    {
        UseServiceProviderFactory(new AutofacServiceProviderFactory());
        var internalHost = base.Build();

        return new RoadRegistryHost<T>(internalHost, _runCommandDelegate);
    }

    public new RoadRegistryHostBuilder<T> ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
    {
        base.ConfigureAppConfiguration((hostContext, services) =>
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (hostContext.HostingEnvironment.IsProduction())
                services
                    .SetBasePath(Directory.GetCurrentDirectory());

            services
                .AddJsonFile("appsettings.json", true, false)
                .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName.ToLowerInvariant()}.json", true, false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
                .AddEnvironmentVariables()
                .AddCommandLine(_args);
            configureDelegate.Invoke(hostContext, services);
        });
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureCommandDispatcher(Func<IServiceProvider, CommandHandlerResolver> configureDelegate)
    {
        base.ConfigureServices((hostContext, services) =>
        {
            services
                .AddSingleton(sp =>
                {
                    var resolver = configureDelegate.Invoke(sp);
                    return Dispatch.Using(resolver);
                });
        });
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureContainer(Action<HostBuilderContext, ContainerBuilder> configureDelegate)
    {
        ConfigureContainer<ContainerBuilder>(configureDelegate.Invoke);
        return this;
    }

    public new RoadRegistryHostBuilder<T> ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
    {
        base.ConfigureHostConfiguration(services =>
        {
            services
                .AddEnvironmentVariables("DOTNET_")
                .AddEnvironmentVariables("ASPNETCORE_");
            configureDelegate.Invoke(services);
        });
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureLogging(Action<HostBuilderContext, ILoggingBuilder> configureDelegate)
    {
        HostingHostBuilderExtensions.ConfigureLogging(this, (hostContext, builder) =>
        {
            SelfLog.Enable(Console.WriteLine);

            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(hostContext.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName();

            Log.Logger = loggerConfiguration.CreateLogger();

            builder.AddSerilog(Log.Logger);
            configureDelegate.Invoke(hostContext, builder);
        });
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureOptions<TOptions>(out TOptions configuredOptions) where TOptions : class, new()
    {
        return ConfigureOptions(typeof(TOptions).Name, out configuredOptions);
    }

    public RoadRegistryHostBuilder<T> ConfigureOptions<TOptions>(string configurationSectionName, out TOptions configuredOptions) where TOptions : class, new()
    {
        var internallyConfiguredOptions = new TOptions();

        base.ConfigureServices((hostContext, services) =>
        {
            var configurationSection = hostContext.Configuration.GetSection(configurationSectionName);

            if (string.IsNullOrEmpty(configurationSection.Value))
                hostContext.Configuration.Bind(internallyConfiguredOptions);
            else
                configurationSection.Bind(internallyConfiguredOptions);

            services.AddSingleton(internallyConfiguredOptions);
        });

        configuredOptions = internallyConfiguredOptions;
        return this;
    }

    public new RoadRegistryHostBuilder<T> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
    {
        ConfigureOptions<BlobClientOptions>(out var blobClientOptions);

        base.ConfigureServices((hostContext, services) =>
        {
            if (blobClientOptions.BlobClientType is not null)
            {
                switch (blobClientOptions.BlobClientType)
                {
                    case nameof(S3BlobClient):
                        var s3Options = new S3BlobClientOptions();
                        hostContext.Configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                        // Use MINIO
                        var minioServer = hostContext.Configuration.GetValue<string>("MINIO_SERVER");
                        if (minioServer != null)
                            services.AddSingleton(new AmazonS3Client(
                                new BasicAWSCredentials(
                                    hostContext.Configuration.GetRequiredValue<string>("MINIO_ACCESS_KEY"),
                                    hostContext.Configuration.GetRequiredValue<string>("MINIO_SECRET_KEY")
                                ),
                                new AmazonS3Config
                                {
                                    RegionEndpoint = RegionEndpoint.USEast1, // minio's default region
                                    ServiceURL = minioServer,
                                    ForcePathStyle = true
                                }
                            ));
                        else // Use AWS
                            services.AddSingleton(new AmazonS3Client());

                        services
                            .AddSingleton<IBlobClient>(sp =>
                                new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.UploadsBucket]
                                )
                            ).AddSingleton(sp =>
                                new RoadNetworkUploadsBlobClient(new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.UploadsBucket]
                                )))
                            .AddSingleton(sp =>
                                new RoadNetworkExtractUploadsBlobClient(new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.UploadsBucket]
                                )))
                            .AddSingleton(sp =>
                                new RoadNetworkExtractDownloadsBlobClient(new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.ExtractDownloadsBucket]
                                )))
                            .AddSingleton(sp =>
                                new RoadNetworkFeatureCompareBlobClient(new S3BlobClient(
                                    sp.GetRequiredService<AmazonS3Client>(),
                                    s3Options.Buckets[WellknownBuckets.FeatureCompareBucket]
                                )))
                            ;

                        break;

                    case nameof(FileBlobClient):
                        var fileOptions = new FileBlobClientOptions();
                        hostContext.Configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                        services
                            .AddSingleton<IBlobClient>(sp =>
                                new FileBlobClient(
                                    new DirectoryInfo(fileOptions.Directory)
                                )
                            )
                            .AddSingleton<RoadNetworkUploadsBlobClient>()
                            .AddSingleton<RoadNetworkExtractUploadsBlobClient>()
                            .AddSingleton<RoadNetworkExtractDownloadsBlobClient>()
                            .AddSingleton<RoadNetworkFeatureCompareBlobClient>();
                        break;

                    default:
                        throw new InvalidOperationException(blobClientOptions.BlobClientType + " is not a supported blob client type.");
                }
            }

            services
                .AddSingleton<Scheduler>()
                .AddStreamStore()
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddRoadRegistrySnapshot()
                .AddSingleton(new RecyclableMemoryStreamManager())
                .AddFeatureToggles<ApplicationFeatureToggle>(hostContext.Configuration)
                .AddSingleton(new WKTReader(
                    new NtsGeometryServices(
                        GeometryConfiguration.GeometryFactory.PrecisionModel,
                        GeometryConfiguration.GeometryFactory.SRID
                    )
                ));

            configureDelegate.Invoke(hostContext, services);
        });
        return this;
    }

    public RoadRegistryHostBuilder<T> ConfigureRunCommand(Func<IServiceProvider, Task> runCommandDelegate)
    {
        _runCommandDelegate = runCommandDelegate;
        return this;
    }
}
