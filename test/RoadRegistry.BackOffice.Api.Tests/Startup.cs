namespace RoadRegistry.BackOffice.Api.Tests;

using Amazon;
using Autofac;
using BackOffice.Extracts;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.Sqs;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Castle.Core.Logging;
using Core;
using Editor.Schema;
using Framework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Product.Schema;
using SqlStreamStore;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;

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
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IZipArchiveAfterFeatureCompareValidator>(),
                sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkCommandModule(
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
                    sp.GetService<IClock>(),
                    sp.GetService<ILoggerFactory>()
                ),
                new RoadNetworkExtractCommandModule(
                    sp.GetService<RoadNetworkExtractUploadsBlobClient>(),
                    sp.GetService<IStreamStore>(),
                    sp.GetService<Func<EventSourcedEntityMap>>(),
                    sp.GetService<IRoadNetworkSnapshotReader>(),
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
        builder.Register<ISqsQueue>((c, p) =>
        {
            var sqsQueueMock = new Mock<ISqsQueue>();
            sqsQueueMock.Setup(x => x.Copy(It.IsAny<SqsRequest>(), It.IsAny<SqsQueueOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => true);
            return sqsQueueMock.Object;
        });
    }

    protected override void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
    {
        services
            .AddTicketing()
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
