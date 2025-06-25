namespace RoadRegistry.BackOffice.Handlers.Tests.Extracts.WhenRequestingRoadNetworkExtractV2;

using Autofac;
using AutoFixture;
using BackOffice.Extracts;
using BackOffice.Framework;
using Editor.Schema;
using Editor.Schema.Extracts;
using ExtractHost;
using FluentAssertions;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;

public class GivenRoadNetwork: RoadNetworkTestBase
{
    public GivenRoadNetwork(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task ThenAnnounceRoadNetworkExtractDownloadBecameAvailable()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        var geometry = new WKTReader().Read("MULTIPOLYGON (((0 0, 0 10, 10 10, 10 0, 0 0)))");
        var overlappingDownloadId = ObjectProvider.Create<Guid>();

        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = overlappingDownloadId,
            Contour = geometry,
            IsInformative = false,
            ExternalRequestId = ObjectProvider.Create<string>(),
            Description = ObjectProvider.Create<string>()
        });
        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = ObjectProvider.Create<Guid>(),
            Contour = geometry,
            IsInformative = true,
            ExternalRequestId = ObjectProvider.Create<string>(),
            Description = ObjectProvider.Create<string>()
        });
        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = ObjectProvider.Create<Guid>(),
            Contour = new WKTReader().Read("MULTIPOLYGON (((1000 0, 1000 10, 1010 10, 1010 0, 1000 0)))"),
            IsInformative = false,
            ExternalRequestId = ObjectProvider.Create<string>(),
            Description = ObjectProvider.Create<string>()
        });
        await editorContext.SaveChangesAsync();

        var eventHandlerModules = new EventHandlerModule[]
        {
            new RoadNetworkExtractEventModule(
                Container,
                new RoadNetworkExtractDownloadsBlobClient(new MemoryBlobClientFactory().Create(ObjectProvider.Create<string>())),
                new RoadNetworkExtractUploadsBlobClient(new MemoryBlobClientFactory().Create(ObjectProvider.Create<string>())),
                new FakeRoadNetworkExtractArchiveAssembler(),
                Store,
                new ApplicationMetadata(RoadRegistryApplication.BackOffice),
                Mock.Of<IRoadNetworkEventWriter>(),
                Mock.Of<IExtractUploadFailedEmailClient>(),
                LoggerFactory.CreateLogger<RoadNetworkExtractEventModule>())
        };

        // Act
        var externalExtractRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var @event = new RoadNetworkExtractGotRequestedV2
        {
            Description = ObjectProvider.Create<string>(),
            DownloadId = ObjectProvider.Create<DownloadId>(),
            RequestId = ExtractRequestId.FromExternalRequestId(externalExtractRequestId),
            ExternalRequestId = externalExtractRequestId,
            IsInformative = false,
            Contour = GeometryTranslator.TranslateToRoadNetworkExtractGeometry((IPolygonal)geometry)
        };
        var dispatcher = Dispatch.Using(Resolve.WhenEqualToMessage(eventHandlerModules));
        await dispatcher(new Event(@event), CancellationToken.None);

        // Assert
        var command = await Store.GetLastMessage<AnnounceRoadNetworkExtractDownloadBecameAvailable>();
        command.DownloadId.Should().Be(@event.DownloadId);
        command.RequestId.Should().Be(@event.RequestId);
        command.IsInformative.Should().BeFalse();
        command.OverlapsWithDownloadIds.Should().BeEquivalentTo([overlappingDownloadId]);
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder.RegisterInstance(BuildEditorContext());
    }

    private EditorContext BuildEditorContext()
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EditorContext(options);
    }
}
