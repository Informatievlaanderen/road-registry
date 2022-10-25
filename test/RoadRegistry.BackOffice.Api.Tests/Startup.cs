namespace RoadRegistry.BackOffice.Api.Tests;

using Amazon;
using Autofac;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Core;
using Editor.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Product.Schema;
using SqlStreamStore;

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

    public override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<MediatorModule>();
        builder.RegisterModule<Handlers.MediatorModule>();
        builder.RegisterModule<Handlers.Sqs.MediatorModule>();

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

    public override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddSingleton(_ => new SqsOptions("", "", RegionEndpoint.EUWest1))
            .AddSingleton<ISqsQueuePublisher>(sp => new FakeSqsQueuePublisher())
            .AddSingleton<ISqsQueueConsumer>(sp => new FakeSqsQueueConsumer())
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