namespace RoadRegistry.Tests;

using System.Reflection;
using System.Text;
using Amazon;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentValidation;
using Hosts.Infrastructure.Modules;
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
using RoadRegistry.BackOffice.Abstractions.Configuration;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extensions;
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

    protected virtual void ConfigureAppConfiguration(HostBuilderContext hostContext, IConfigurationBuilder builder)
    {
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
                    
                    .AddSingleton<IStreamStore>(sp => new InMemoryStreamStore())
                    .AddSingleton<IStreetNameCache>(_ => new FakeStreetNameCache())
                    .AddSingleton<IClock>(new FakeClock(NodaConstants.UnixEpoch))
                    .AddSingleton<Func<EventSourcedEntityMap>>(_ => () => new EventSourcedEntityMap())
                    .AddSingleton(ConfigureCommandHandlerDispatcher)
                    .AddSingleton(new RecyclableMemoryStreamManager())
                    .AddSingleton(new ZipArchiveWriterOptions())
                    .AddSingleton(new ExtractDownloadsOptions())
                    .AddSingleton(new ExtractUploadsOptions())
                    .AddSingleton(new FeatureCompareMessagingOptions
                    {
                        RequestQueueUrl = "request.fifo",
                        ResponseQueueUrl = "response.fifo"
                    })
                    .AddTransient<IZipArchiveBeforeFeatureCompareValidator>(sp => new ZipArchiveBeforeFeatureCompareValidator(Encoding.UTF8))
                    .AddTransient<IZipArchiveAfterFeatureCompareValidator>(sp => new ZipArchiveAfterFeatureCompareValidator(Encoding.UTF8))
                    .AddValidatorsFromAssemblies(availableModuleAssemblyCollection)
                    .AddFeatureToggles<ApplicationFeatureToggle>(context.Configuration)
                    .AddLogging();

                ConfigureServices(context, services);
            })
            .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
            {
                builder.RegisterAssemblyModules(availableModuleAssemblyCollection.ToArray());

                builder.Register<IRoadNetworkSnapshotWriter>(sp => new FakeRoadNetworkSnapshotWriter()).SingleInstance();
                builder.Register<IRoadNetworkSnapshotReader>(sp => new FakeRoadNetworkSnapshotReader()).SingleInstance();

                ConfigureContainer(hostContext, builder);
                ConfigureContainer(builder);

                builder
                    .Register(c => new SqsOptions(RegionEndpoint.EUWest1, EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
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

    private static IEnumerable<Assembly> DetermineAvailableAssemblyCollection()
    {
        var executorAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executorDirectoryInfo = new DirectoryInfo(executorAssemblyLocation).Parent;
        var assemblyFileInfoCollection = executorDirectoryInfo.EnumerateFiles("RoadRegistry.*.dll");
        var assemblyCollection = assemblyFileInfoCollection.Select(fi => Assembly.LoadFrom(fi.FullName));
        return assemblyCollection.ToList();
    }
}
