namespace RoadRegistry.BackOffice.EventHost.Tests;

using AutoFixture;
using BackOffice.Framework;
using Core;
using MediatR;
using Messages;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Snapshot.Handlers;
using Snapshot.Handlers.Sqs.RoadNetworks;
using Xunit.Abstractions;

public class RoadNetworkSnapshotEventModuleTests : RoadNetworkTestBase
{
    private readonly Mock<IMediator> _mediator;

    public RoadNetworkSnapshotEventModuleTests(ITestOutputHelper testOutputHelper)
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
        var dispatcher = Dispatch.Using(Resolve.WhenEqualToMessage([
            new RoadNetworkSnapshotEventModule(
                Store,
                ScopedContainer,
                _mediator.Object,
                new FakeRoadNetworkSnapshotReader(),
                new FakeRoadNetworkSnapshotWriter(),
                Clock,
                LoggerFactory
            )
        ]));
        await dispatcher(@event, CancellationToken.None);
    }
}
