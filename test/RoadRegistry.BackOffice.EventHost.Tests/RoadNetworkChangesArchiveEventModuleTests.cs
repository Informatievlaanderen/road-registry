namespace RoadRegistry.BackOffice.EventHost.Tests;

using System.IO.Compression;
using AutoFixture;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Extracts;
using FeatureCompare;
using FluentAssertions;
using Handlers.Uploads;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoadRegistry.Extracts;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V1;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Uploads;
using Xunit.Abstractions;

public class RoadNetworkChangesArchiveEventModuleTests : RoadNetworkTestBase
{
    private readonly Mock<IRoadNetworkEventWriter> _roadNetworkEventWriterMock;

    public RoadNetworkChangesArchiveEventModuleTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _roadNetworkEventWriterMock = new Mock<IRoadNetworkEventWriter>();

        ObjectProvider.Customize<RoadNetworkChangesArchiveAccepted>(customizer =>
            customizer.FromFactory(_ => new RoadNetworkChangesArchiveAccepted
            {
                ArchiveId = ObjectProvider.Create<ArchiveId>(),
                ExtractRequestId = ObjectProvider.Create<ExtractRequestId>(),
                Description = ObjectProvider.Create<string>(),
                DownloadId = ObjectProvider.Create<DownloadId>(),
                Problems = []
            }).OmitAutoProperties());
    }

    [Fact]
    public async Task WhenRoadNetworkChangesArchiveAcceptedWithEmptyChanges_ThenRoadNetworkChangesArchiveRejected()
    {
        var archiveAccepted = ObjectProvider.Create<RoadNetworkChangesArchiveAccepted>();

        await Given(RoadNetworkExtracts.ToStreamName(ExtractRequestId.FromString(archiveAccepted.ExtractRequestId)),
            new RoadNetworkExtractDownloadBecameAvailable
            {
                DownloadId = archiveAccepted.DownloadId!.Value,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.V2
            });

        var @event = new Event(archiveAccepted)
            .WithProvenanceData(new RoadRegistryProvenanceData());
        await DispatchEvent(@event);

        var producedEvent = (Event)_roadNetworkEventWriterMock.Invocations.Single().Arguments[2];
        producedEvent.Body.Should().BeOfType<RoadNetworkChangesArchiveRejected>();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(_ =>
            {
                var translator = new Mock<IZipArchiveFeatureCompareTranslator>();
                translator
                    .Setup(x => x.TranslateAsync(It.IsAny<ZipArchive>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(() => TranslatedChanges.Empty);

                return translator.Object;
            })
            .AddSingleton(sp =>
            {
                var translator = new Mock<IZipArchiveFeatureCompareTranslatorFactory>();
                translator
                    .Setup(x => x.Create(It.IsAny<string>()))
                    .Returns(sp.GetRequiredService<IZipArchiveFeatureCompareTranslator>);

                return translator.Object;
            });
    }

    private async Task DispatchEvent(Event @event)
    {
        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                return new BlobObject(new BlobName("archive.zip"), Metadata.None, new ContentType(), _ =>
                {
                    var archiveStream = new ExtractV1ZipArchiveBuilder()
                        .BuildArchiveStream();
                    return Task.FromResult<Stream>(archiveStream);
                });
            });

        var dispatcher = Dispatch.Using(Resolve.WhenEqualToMessage([
            new RoadNetworkChangesArchiveEventModule(
                ScopedContainer,
                new RoadNetworkUploadsBlobClient(blobClient.Object),
                Store,
                new ApplicationMetadata(RoadRegistryApplication.BackOffice),
                _roadNetworkEventWriterMock.Object,
                LoggerFactory
            )
        ]));
        await dispatcher(@event, CancellationToken.None);
    }
}
