namespace RoadRegistry.BackOffice.MessagingHost.Kafka.Tests;

using Autofac;
using Core;
using Editor.Schema;
using Extracts;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Product.Schema;
using SqlStreamStore;
using Uploads;

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
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                    sp.GetService<IClock>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IRoadNetworkSnapshotWriter>(),
                    sp.GetService<IClock>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                    sp.GetService<IClock>()
                )
            }));
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        //builder.Register(ctx =>
        //{
        //    var loggerFactory = ctx.Resolve<ILoggerFactory>();
        //    var services = ctx.Resolve<IServiceCollection>();
        //    builder.RegisterModule(new Kafka.ConsumerModule(ctx.Resolve<IConfiguration>(), ctx.Resolve<IServiceCollection>(), loggerFactory));
        //}).As<IConfiguration>();
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
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