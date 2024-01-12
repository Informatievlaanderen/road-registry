namespace RoadRegistry.BackOffice.MessagingHost.Sqs.Tests;

using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Core;
using Editor.Schema;
using Extracts;
using Framework;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using Product.Schema;
using RoadRegistry.BackOffice.FeatureToggles;
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
                    new UseOvoCodeInChangeRoadNetworkFeatureToggle(true),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetService<IRoadNetworkEventWriter>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<ILifetimeScope>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveBeforeFeatureCompareValidator>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                    sp.GetService<IExtractUploadFailedEmailClient>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                )
            }));
    }

    protected override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<MediatorModule>();

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

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddSingleton<SqsOptions>(_ => new FakeSqsOptions())
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
            .AddSingleton(sp => new SqsQueueOptions
            {
                CreateQueueIfNotExists = true,
                MessageGroupId = "TEST"
            });
    }
}
