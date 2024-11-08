namespace RoadRegistry.Tests;

using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentValidation;
using Infrastructure.Modules;
using MartinCostello.Logging.XUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.FeatureCompare.Validation;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.BackOffice.ZipArchiveWriters.Cleaning;
using SqlStreamStore;

public abstract class TestStartup
{
    public void Configure(ILoggerFactory loggerFactory, ITestOutputHelperAccessor accessor)
    {
        loggerFactory.AddProvider(new XUnitLoggerProvider(accessor, new XUnitLoggerOptions()));
    }

    protected virtual void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder builder)
    {
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
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false);

                ConfigureAppConfiguration(hostContext, configurationBuilder);
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
                    .AddSingleton<IRoadNetworkSnapshotWriter>(sp => new FakeRoadNetworkSnapshotWriter())
                    .AddSingleton<IRoadNetworkSnapshotReader>(sp => new FakeRoadNetworkSnapshotReader())
                    .AddSingleton<IExtractUploadFailedEmailClient>(sp => new FakeExtractUploadFailedEmailClient())
                    .AddSingleton<IStreamStore>(sp => new InMemoryStreamStore())
                    .AddSingleton<IStreetNameCache>(_ => new FakeStreetNameCache())
                    .AddSingleton<IClock>(new FakeClock(NodaConstants.UnixEpoch))
                    .AddEventEnricher()
                    .AddScoped(_ => new EventSourcedEntityMap())
                    .AddSingleton(ConfigureCommandHandlerDispatcher)
                    .AddSingleton(new RecyclableMemoryStreamManager())
                    .AddSingleton(new ZipArchiveWriterOptions())
                    .AddSingleton(new ExtractDownloadsOptions())
                    .AddSingleton(new ExtractUploadsOptions())
                    .AddSingleton(FileEncoding.UTF8)
                    .AddSingleton<IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
                    .AddTransient<IZipArchiveBeforeFeatureCompareValidator, ZipArchiveBeforeFeatureCompareValidator>()
                    .AddSingleton<IBeforeFeatureCompareZipArchiveCleaner, BeforeFeatureCompareZipArchiveCleaner>()
                    .AddFeatureCompare()
                    .AddValidatorsFromAssemblies(availableModuleAssemblyCollection)
                    .AddFeatureToggles<ApplicationFeatureToggle>(context.Configuration)
                    .AddLogging(c => c.AddXUnit())
                    .AddRoadNetworkCommandQueue()
                    .AddOrganizationCommandQueue()
                    ;

                ConfigureServices(context, services);
            })
            .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
            {
                builder.RegisterAssemblyModules(availableModuleAssemblyCollection.ToArray());

                ConfigureContainer(hostContext, builder);
                ConfigureContainer(builder);

                builder.RegisterModule<BlobClientTestModule>();

                builder
                    .Register(c => new FakeSqsOptions())
                    .As<SqsOptions>()
                    .SingleInstance();

                builder
                    .Register(c => new FakeSqsQueuePublisher())
                    .As<ISqsQueuePublisher>()
                    .SingleInstance();

                builder
                    .Register(c => new FakeSqsQueueConsumer())
                    .As<ISqsQueueConsumer>()
                    .SingleInstance();
            });
    }

    protected virtual void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
    }

    protected virtual IEnumerable<Assembly> DetermineAvailableAssemblyCollection() => Enumerable.Empty<Assembly>();
}
