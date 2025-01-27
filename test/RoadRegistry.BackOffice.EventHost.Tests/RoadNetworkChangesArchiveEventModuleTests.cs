namespace RoadRegistry.BackOffice.EventHost.Tests;

using AutoFixture;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Core;
using FeatureCompare.Readers;
using Handlers.Uploads;
using MediatR;
using Messages;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Snapshot.Handlers.Sqs.RoadNetworks;
using Uploads;
using Xunit.Abstractions;

public class RoadNetworkChangesArchiveEventModuleTests : RoadNetworkTestBase
{
    private readonly Mock<IMediator> _mediator;

    public RoadNetworkChangesArchiveEventModuleTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _mediator = new Mock<IMediator>();
    }

    [Fact]
    public async Task WhenRoadNetworkChangesAccepted_ThenRequestSnapshot()
    {
        var streamId = RoadNetworkStreamNameProvider.Default;
        var @event = new Event(TestData.ObjectProvider.Create<RoadNetworkChangesAccepted>())
            .WithStream(streamId, TestData.ObjectProvider.Create<int>());

        await DispatchEvent(@event);

        _mediator.Verify(
            x => x.Send(It.IsAny<CreateRoadNetworkSnapshotSqsRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task WhenOutlinedRoadNetworkChangesAccepted_ThenNone()
    {
        var streamId = TestData.ObjectProvider.Create<string>();
        var @event = new Event(TestData.ObjectProvider.Create<RoadNetworkChangesAccepted>())
            .WithStream(streamId, TestData.ObjectProvider.Create<int>());

        await DispatchEvent(@event);

        _mediator.Verify(
            x => x.Send(It.IsAny<CreateRoadNetworkSnapshotSqsRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private async Task DispatchEvent(Event @event)
    {
        var blobClient = new Mock<IBlobClient>();
        // blobClient
        //     .Setup()

        var dispatcher = Dispatch.Using(Resolve.WhenEqualToMessage([
            new RoadNetworkChangesArchiveEventModule(
                ScopedContainer,
                new RoadNetworkUploadsBlobClient(blobClient.Object),
                Store,
                new ApplicationMetadata(RoadRegistryApplication.BackOffice),
                Mock.Of<ITransactionZoneFeatureCompareFeatureReader>(),
                Mock.Of<IRoadNetworkEventWriter>(),
                Mock.Of<IExtractUploadFailedEmailClient>(),
                LoggerFactory
            )
        ]));
        await dispatcher(@event, CancellationToken.None);
    }
}
