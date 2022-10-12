using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoadRegistry.Hosts
{
    using Amazon.Runtime;
    using Amazon.S3;
    using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
    using Be.Vlaanderen.Basisregisters.BlobStore.IO;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IO;
    using NodaTime;
    using RoadRegistry.BackOffice.Core;
    using RoadRegistry.BackOffice.Extracts;
    using RoadRegistry.BackOffice.Uploads;
    using Serilog.Debugging;
    using Serilog;
    using SqlStreamStore;
    using System.IO;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using BackOffice.Framework;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Configuration;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using System.Configuration;
    using System.Reflection.Metadata;
    using Amazon;
    using Extensions.Configuration;

    public sealed class RoadRegistryHostBuilder<T> : HostBuilder
    {
        private string[] _args;

        private RoadRegistryHostBuilder()
        {
            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(eventArgs.Exception, "FirstChanceException event raised in {AppDomain}.", AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            this
                .ConfigureHostConfiguration(builder =>
                {
                })
                .ConfigureAppConfiguration((hostContext, builder) =>
                {
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                });
        }

        public RoadRegistryHostBuilder(string[] args) : this() => _args = args;

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

        public RoadRegistryHostBuilder<T> ConfigureOptions<TOptions>(out TOptions configuredOptions) where TOptions : class, new() => ConfigureOptions(typeof(TOptions).Name, out configuredOptions);

        public RoadRegistryHostBuilder<T> ConfigureOptions<TOptions>(string configurationSectionName, out TOptions configuredOptions) where TOptions : class, new()
        {
            var internallyConfiguredOptions = new TOptions();

            base.ConfigureServices((hostContext, services) =>
            {
                var configurationSection = hostContext.Configuration.GetSection(configurationSectionName);

                if (string.IsNullOrEmpty(configurationSection.Value))
                {
                    hostContext.Configuration.Bind(internallyConfiguredOptions);
                }
                else
                {
                    configurationSection.Bind(internallyConfiguredOptions);
                }

                services.AddSingleton(internallyConfiguredOptions);
            });

            configuredOptions = internallyConfiguredOptions;
            return this;
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

        public new RoadRegistryHostBuilder<T> ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            ConfigureOptions<BlobClientOptions>(out var blobClientOptions);

            base.ConfigureServices((hostContext, services) =>
            {
                switch (blobClientOptions.BlobClientType)
                {
                    case nameof(S3BlobClient):
                        var s3Options = new S3BlobClientOptions();
                        hostContext.Configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                        // Use MINIO
                        var minioServer = hostContext.Configuration.GetValue<string>("MINIO_SERVER");
                        if (minioServer != null)
                        {
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
                        }
                        else // Use AWS
                        {
                            services.AddSingleton(new AmazonS3Client(
                                new BasicAWSCredentials(
                                    hostContext.Configuration.GetRequiredValue<string>("AWS_ACCESS_KEY_ID"),
                                    hostContext.Configuration.GetRequiredValue<string>("AWS_SECRET_ACCESS_KEY")
                                )
                            ));
                        }

                        services
                            .AddSingleton(sp =>
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

                services
                    .AddSingleton<Scheduler>()
                    .AddSingleton<IStreamStore>(sp =>
                        new MsSqlStreamStoreV3(
                            new MsSqlStreamStoreV3Settings(
                                sp
                                    .GetService<IConfiguration>()
                                    .GetConnectionString(WellknownConnectionNames.Events))
                            {
                                Schema = WellknownSchemas.EventSchema
                            }))
                    .AddSingleton<IClock>(SystemClock.Instance)
                    .AddSingleton(new RecyclableMemoryStreamManager())
                    .AddSingleton(sp => new RoadNetworkSnapshotReaderWriter(
                        new RoadNetworkSnapshotsBlobClient(
                            new SqlBlobClient(
                                new SqlConnectionStringBuilder(
                                    sp
                                        .GetService<IConfiguration>()
                                        .GetConnectionString(WellknownConnectionNames.Snapshots)),
                                WellknownSchemas.SnapshotSchema)),
                        sp.GetService<RecyclableMemoryStreamManager>()))
                    .AddSingleton<IRoadNetworkSnapshotReader>(sp => sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>())
                    .AddSingleton<IRoadNetworkSnapshotWriter>(sp => sp.GetRequiredService<RoadNetworkSnapshotReaderWriter>());
                configureDelegate.Invoke(hostContext, services);
            });
            return this;
        }

        public RoadRegistryHostBuilder<T> ConfigureContainer(Action<HostBuilderContext, ContainerBuilder> configureDelegate)
        {
            ConfigureContainer<ContainerBuilder>(configureDelegate.Invoke);
            return this;
        }

        public new RoadRegistryHost<T> Build()
        {
            UseServiceProviderFactory(new AutofacServiceProviderFactory());
            var internalHost = base.Build();

            return new RoadRegistryHost<T>(internalHost);
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
    }
}
