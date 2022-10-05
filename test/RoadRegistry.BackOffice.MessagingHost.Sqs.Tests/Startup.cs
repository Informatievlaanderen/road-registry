namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests;

using Amazon;
using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Product.Schema;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Tests;
using SqlStreamStore;
using IClock = NodaTime.IClock;

public class Startup : TestStartup
{
    public override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services) => services
        .AddSingleton<SqsOptions>(_ => new SqsOptions("", "", RegionEndpoint.EUWest1))
        .AddSingleton<ISqsQueuePublisher>(sp => new FakeSqsQueuePublisher())
        .AddSingleton<ISqsQueueConsumer>(sp => new FakeSqsQueueConsumer())
        .AddDbContext<EditorContext>((sp, options) => options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N")))
        .AddDbContext<ProductContext>((sp, options) => options
            .UseLoggerFactory(sp.GetService<ILoggerFactory>())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N")))
        .AddSingleton<SqsQueueOptions>(sp => new SqsQueueOptions()
        {
            CreateQueueIfNotExists = true,
            MessageGroupId = "TEST"
        });

    public override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<RoadRegistry.BackOffice.MessagingHost.Sqs.MediatorModule>();

        //builder.Register<SqlServer>(sp => new SqlServer());
        //builder.Register(ctx =>
        //{
        //    var sqlServer = ctx.Resolve<SqlServer>();
        //    var database = sqlServer.CreateDatabaseAsync().GetAwaiter().GetResult();
        //    var context = sqlServer.CreateEmptyEditorContextAsync(database).GetAwaiter().GetResult();
        //    return context;
        //}).As<EditorContext>().;
        //builder.Register(ctx =>
        //{
        //    var sqlServer = ctx.Resolve<SqlServer>();
        //    var database = sqlServer.CreateDatabaseAsync().GetAwaiter().GetResult();
        //    var context = sqlServer.CreateEmptyProductContextAsync(database).GetAwaiter().GetResult();
        //    return context;
        //}).As<ProductContext>();
    }

    protected override CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp) =>
        Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkFeatureCompareBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
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
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetService<IClock>()
                )
            }));
}
