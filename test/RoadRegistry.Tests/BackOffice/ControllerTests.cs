namespace RoadRegistry.BackOffice;

using System;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Core;
using Editor.Schema;
using Extracts;
using Framework;
using Handlers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Moq;
using NetTopologySuite;
using NetTopologySuite.IO;
using NodaTime;
using Scenarios;
using SqlStreamStore;
using Uploads;

public abstract class ControllerTests<TController> where TController : ControllerBase
{
    protected ControllerTests()
    {
        EditorContext = ConfigureEditorContextMock();

        var services = new ServiceCollection();
        services.AddSingleton(EditorContext.Object);

        Container = ConfigureDependencyInjectionContainer(services);

        var mediator = Container.Resolve<IMediator>();
        Controller = (TController)Activator.CreateInstance(typeof(TController), mediator);
        Controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        StreamStore = Container.Resolve<IStreamStore>();
        UploadBlobClient = Container.Resolve<RoadNetworkUploadsBlobClient>();
        ExtractUploadClient = Container.Resolve<RoadNetworkExtractUploadsBlobClient>();
    }

    protected IStreamStore StreamStore { get; }
    protected IMock<EditorContext> EditorContext { get; }
    public RoadNetworkUploadsBlobClient UploadBlobClient { get; }
    protected RoadNetworkExtractUploadsBlobClient ExtractUploadClient { get; }
    protected IContainer Container { get; }

    protected TController Controller { get; init; }

    protected virtual Mock<EditorContext> ConfigureEditorContextMock()
    {
        var mock = new Mock<EditorContext>();
        return mock;
    }

    private IContainer ConfigureDependencyInjectionContainer(IServiceCollection services)
    {
        services
            .AddDbContext<EditorContext>((sp, options) => options
                .UseLoggerFactory(sp.GetService<ILoggerFactory>())
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
            );

        services
            .AddSingleton<IStreamStore>(sp => new InMemoryStreamStore())
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddSingleton(new WKTReader(
                new NtsGeometryServices(
                    GeometryConfiguration.GeometryFactory.PrecisionModel,
                    GeometryConfiguration.GeometryFactory.SRID
                )
            ))
            .AddSingleton(new RecyclableMemoryStreamManager())
            .AddSingleton<IBlobClient>(new MemoryBlobClient())
            .AddLogging();

        services
            .AddSingleton(sp =>
                new RoadNetworkUploadsBlobClient(sp.GetService<IBlobClient>()))
            .AddSingleton(sp =>
                new RoadNetworkExtractUploadsBlobClient(sp.GetService<IBlobClient>()))
            .AddSingleton(sp =>
                new RoadNetworkExtractDownloadsBlobClient(sp.GetService<IBlobClient>()));

        var fakeSnapshotReader = new FakeRoadNetworkSnapshotReader();
        services
            .AddSingleton<IRoadNetworkSnapshotWriter>(sp => new RoadNetworkSnapshotReaderWriter(
                new RoadNetworkSnapshotsBlobClient(sp.GetService<IBlobClient>()),
                sp.GetService<RecyclableMemoryStreamManager>()))
            .AddSingleton<IRoadNetworkSnapshotReader>(fakeSnapshotReader)
            .AddSingleton(sp => Dispatch.Using(Resolve.WhenEqualToMessage(
                new CommandHandlerModule[]
                {
                    new RoadNetworkChangesArchiveCommandModule(
                        sp.GetService<RoadNetworkUploadsBlobClient>(),
                        sp.GetService<IStreamStore>(),
                        sp.GetService<IRoadNetworkSnapshotReader>(),
                        new ZipArchiveValidator(Encoding.UTF8),
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
                        new ZipArchiveValidator(Encoding.UTF8),
                        sp.GetService<IClock>()
                    )
                })));

        var builder = new ContainerBuilder();
        builder.Populate(services);
        builder.RegisterModule<MediatRModule>();

        builder.Register(ctx => Dispatch.Using(
                Resolve.WhenEqualToMessage(
                    new CommandHandlerModule[]
                    {
                        new RoadNetworkChangesArchiveCommandModule(
                            ctx.Resolve<RoadNetworkUploadsBlobClient>(),
                            ctx.Resolve<IStreamStore>(),
                            ctx.Resolve<IRoadNetworkSnapshotReader>(),
                            new ZipArchiveValidator(Encoding.UTF8),
                            ctx.Resolve<IClock>()
                        ),
                        new RoadNetworkCommandModule(
                            ctx.Resolve<IStreamStore>(),
                            ctx.Resolve<IRoadNetworkSnapshotReader>(),
                            ctx.Resolve<IRoadNetworkSnapshotWriter>(),
                            ctx.Resolve<IClock>()
                        ),
                        new RoadNetworkExtractCommandModule(
                            ctx.Resolve<RoadNetworkExtractUploadsBlobClient>(),
                            ctx.Resolve<IStreamStore>(),
                            ctx.Resolve<IRoadNetworkSnapshotReader>(),
                            new ZipArchiveValidator(Encoding.UTF8),
                            ctx.Resolve<IClock>()
                        )
                    })
            )
        );

        return builder.Build();
    }
}
