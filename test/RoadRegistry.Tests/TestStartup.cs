namespace RoadRegistry.Tests;

using System.Reflection;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.ZipArchiveWriters.Validation;
using SqlStreamStore;
using Xunit.DependencyInjection;
using Xunit.DependencyInjection.Logging;

public abstract class TestStartup
{
    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor)
    {
        loggerFactory.AddProvider(new XunitTestOutputLoggerProvider(accessor));
    }

    protected virtual CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp)
    {
        return default;
    }

    protected virtual void ConfigureContainer(ContainerBuilder builder)
    {
    }
    protected virtual void ConfigureContainer(HostBuilderContext hostContext, ContainerBuilder builder)
    {
    }

    public void ConfigureHost(IHostBuilder hostBuilder)
    {
        var availableModuleAssemblyCollection = DetermineAvailableAssemblyCollection();

        hostBuilder
            .ConfigureAppConfiguration((hostContext, configurationBuilder) =>
            {
                configurationBuilder
                    .AddJsonFile("appsettings.json", true, true);
            })
            .UseServiceProviderFactory(new AutofacServiceProviderFactory())
            .ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton(new WKTReader(
                        new NtsGeometryServices(
                            GeometryConfiguration.GeometryFactory.PrecisionModel,
                            GeometryConfiguration.GeometryFactory.SRID
                        )
                    ))
                    .AddSingleton<IBlobClient>(new MemoryBlobClient())
                    .AddSingleton(sp => new RoadNetworkUploadsBlobClient(sp.GetService<IBlobClient>()))
                    .AddSingleton(sp => new RoadNetworkExtractUploadsBlobClient(sp.GetService<IBlobClient>()))
                    .AddSingleton(sp => new RoadNetworkExtractDownloadsBlobClient(sp.GetService<IBlobClient>()))
                    .AddSingleton(sp => new RoadNetworkFeatureCompareBlobClient(sp.GetService<IBlobClient>()))
                    .AddSingleton(sp => new RoadNetworkSnapshotsBlobClient(sp.GetService<IBlobClient>()))
                    .AddSingleton<IStreamStore>(sp => new InMemoryStreamStore())
                    .AddSingleton<IStreetNameCache>(_ => new FakeStreetNameCache())
                    .AddSingleton<IClock>(SystemClock.Instance)
                    .AddSingleton<IRoadNetworkSnapshotWriter>(sp => new FakeRoadNetworkSnapshotWriter())
                    .AddSingleton<IRoadNetworkSnapshotReader>(sp => new FakeRoadNetworkSnapshotReader())
                    .AddSingleton(ConfigureCommandHandlerDispatcher)
                    .AddSingleton(new RecyclableMemoryStreamManager())
                    .AddSingleton(new ZipArchiveWriterOptions())
                    .AddSingleton(new ExtractDownloadsOptions())
                    .AddSingleton(new ExtractUploadsOptions())
                    //.AddSingleton<SqsOptions>(_ => new SqsOptions("", "", RegionEndpoint.EUWest1))
                    .AddTransient<IZipArchiveBeforeFeatureCompareValidator>(sp => new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
                    .AddTransient<IZipArchiveAfterFeatureCompareValidator>(sp => new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8))
                    .AddValidatorsFromAssemblies(availableModuleAssemblyCollection)
                    .AddSingleton(c => new UseSnapshotRebuildFeatureToggle(c.GetRequiredService<IOptions<FeatureToggleOptions>>().Value.UseSnapshotRebuild))
                    .AddSingleton(c => new UseFeatureCompareFeatureToggle(c.GetRequiredService<IOptions<FeatureToggleOptions>>().Value.UseFeatureCompare))
                    .AddSingleton(c => new UseApiKeyAuthenticationFeatureToggle(c.GetRequiredService<IOptions<FeatureToggleOptions>>().Value.UseApiKeyAuthentication))
                    .AddSingleton(c => new UseUploadZipArchiveValidationFeatureToggle(c.GetRequiredService<IOptions<FeatureToggleOptions>>().Value.UseUploadZipArchiveValidation))
                    .AddLogging();

                ConfigureServices(context, services);
            })
            .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
            {
                ConfigureContainer(hostContext, builder);
                ConfigureContainer(builder);

                builder.RegisterAssemblyModules(availableModuleAssemblyCollection.ToArray());
            });
    }

    protected virtual void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
    }

    private static IEnumerable<Assembly> DetermineAvailableAssemblyCollection()
    {
        var executorAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executorDirectoryInfo = new DirectoryInfo(executorAssemblyLocation).Parent;
        var assemblyFileInfoCollection = executorDirectoryInfo.EnumerateFiles("RoadRegistry.*.dll");
        var assemblyCollection = assemblyFileInfoCollection.Select(fi => Assembly.LoadFrom(fi.FullName));
        return assemblyCollection.ToList();
    }
}