namespace RoadRegistry.BackOffice.Api.Tests;

using Amazon;
using Autofac;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Editor.Schema;
using Framework;
using Framework.Containers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Product.Schema;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Uploads;
using SqlStreamStore;
using IClock = NodaTime.IClock;

public class Startup : TestStartup
{
    public override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services) => services
        .AddSingleton<SqsOptions>(_ => new SqsOptions("", "", RegionEndpoint.EUWest1))
        .AddSingleton<ISqsQueuePublisher>(sp => MockStartup.ConfigureMockQueuePublisher().Object)
        .AddSingleton<ISqsQueueConsumer>(sp => MockStartup.ConfigureMockQueueConsumer().Object)
    ;

    public override void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule<RoadRegistry.BackOffice.MediatorModule>();
        builder.Register<SqlServer>(sp => new SqlServer());
        builder.Register(ctx =>
        {
            var sqlServer = ctx.Resolve<SqlServer>();
            var database = sqlServer.CreateDatabaseAsync().GetAwaiter().GetResult();
            var context = sqlServer.CreateEmptyEditorContextAsync(database).GetAwaiter().GetResult();
            return context;
        }).As<EditorContext>();
        builder.Register(ctx =>
        {
            var sqlServer = ctx.Resolve<SqlServer>();
            var database = sqlServer.CreateDatabaseAsync().GetAwaiter().GetResult();
            var context = sqlServer.CreateEmptyProductContextAsync(database).GetAwaiter().GetResult();
            return context;
        }).As<ProductContext>();
    }

    protected override CommandHandlerDispatcher ConfigureCommandHandlerDispatcher(IServiceProvider sp) =>
        Dispatch.Using(Resolve.WhenEqualToMessage(
            new CommandHandlerModule[]
            {
                new RoadNetworkChangesArchiveCommandModule(
                    sp.GetService<RoadNetworkUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveValidator>(),
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
                    sp.GetService<IZipArchiveValidator>(),
                    sp.GetService<IClock>()
                )
            }));
}

public static class MockStartup
{
    internal static Mock<ISqsQueuePublisher> ConfigureMockQueuePublisher()
    {
        var mock = new Mock<ISqsQueuePublisher>();
        mock.Setup(publisher => publisher.CopyToQueue(It.IsAny<string>(), It.IsAny<It.IsAnyType>(), It.IsAny<SqsQueueOptions>(), It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true));
        return mock;
    }

    internal static Mock<ISqsQueueConsumer> ConfigureMockQueueConsumer()
    {
        var mock = new Mock<ISqsQueueConsumer>();
        return mock;
    }
}
