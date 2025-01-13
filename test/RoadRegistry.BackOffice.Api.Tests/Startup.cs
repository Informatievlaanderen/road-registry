namespace RoadRegistry.BackOffice.Api.Tests;

using Api.Changes;
using Api.Downloads;
using Api.Extracts;
using Api.Information;
using Api.RoadSegments;
using Api.Uploads;
using Autofac;
using BackOffice.Extracts;
using BackOffice.Handlers.Sqs;
using BackOffice.Uploads;
using Core;
using Editor.Schema;
using Extensions;
using FeatureToggles;
using Framework;
using Hosts.Infrastructure.Extensions;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Product.Schema;
using RoadRegistry.BackOffice.Api.Organizations;
using RoadRegistry.Hosts.Infrastructure.Options;
using SqlStreamStore;
using System.Reflection;
using Api.Grb;
using FeatureCompare.Readers;
using FeatureCompare.Translators;
using Sync.MunicipalityRegistry;
using MediatorModule = BackOffice.MediatorModule;

public class Startup : TestStartup
{
    protected override CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp)
    {
        return Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetService<ITransactionZoneFeatureCompareFeatureReader>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IClock>(),
                    new UseOvoCodeInChangeRoadNetworkFeatureToggle(true),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }));
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<MediatorModule>();
        builder.RegisterModule<BackOffice.Handlers.MediatorModule>();
        builder.RegisterModule<BackOffice.Handlers.Sqs.MediatorModule>();

        builder
            .RegisterInstance(new FakeBackOfficeS3SqsQueue())
            .As<IBackOfficeS3SqsQueue>();
        builder
            .RegisterInstance(new FakeRoadSegmentFeatureCompareStreetNameContextFactory())
            .As<IRoadSegmentFeatureCompareStreetNameContextFactory>();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddTicketing()
            .AddFakeTicketing()
            .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
            .AddRoadNetworkCommandQueue()
            .AddRoadNetworkEventWriter()
            .AddInMemoryDbContext<EditorContext>()
            .AddInMemoryDbContext<ProductContext>()
            .AddInMemoryDbContext<MunicipalityEventConsumerContext>()
            .AddSingleton<TicketingOptions>(new FakeTicketingOptions())
            .AddScoped<ChangeFeedController>()
            .AddScoped<DownloadController>()
            .AddScoped<ExtractsController>()
            .AddScoped<GrbController>()
            .AddScoped<InformationController>()
            .AddScoped<OrganizationsController>()
            .AddScoped<UploadController>()
            .AddScoped<IRoadSegmentRepository, RoadSegmentRepository>()
            ;
    }

    protected override IEnumerable<Assembly> DetermineAvailableAssemblyCollection()
    {
        var executorAssemblyLocation = Assembly.GetExecutingAssembly().Location;
        var executorDirectoryInfo = new DirectoryInfo(executorAssemblyLocation).Parent;
        var assemblyFileInfoCollection = executorDirectoryInfo.EnumerateFiles("RoadRegistry.*.dll");
        var assemblyCollection = assemblyFileInfoCollection.Select(fi => Assembly.LoadFrom(fi.FullName));
        return assemblyCollection.ToList();
    }
}
