namespace RoadRegistry.BackOffice.Api.Tests;

using Autofac;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Core;
using Editor.Schema;
using Framework.Extensions;
using Handlers.Sqs;
using Hosts.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NodaTime;
using Product.Schema;
using RoadRegistry.BackOffice.Extensions;
using SqlStreamStore;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
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
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }));
    }
    
    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<MediatorModule>();
        builder.RegisterModule<Handlers.MediatorModule>();
        builder.RegisterModule<Handlers.Sqs.MediatorModule>();

        builder
            .RegisterInstance(new FakeBackOfficeS3SqsQueue())
            .As<IBackOfficeS3SqsQueue>();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddTicketing()
            .AddFakeTicketing()
            .AddSingleton(new ApplicationMetadata(RoadRegistryApplication.BackOffice))
            .AddRoadNetworkCommandQueue()
            .AddRoadNetworkEventWriter()
            .AddDbContext<EditorContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")))
            .AddDbContext<ProductContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N")));
    }
}
