namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments;

using Abstractions.RoadSegments;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using FluentValidation;
using Handlers.RoadSegments;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore;
using StreetNameConsumer.Schema;
using AcceptedChange = Messages.AcceptedChange;

public class LinkRoadSegmentToStreetNameRequestHandlerTests
{
    private RoadNetworkFixture Fixture { get; }
    private LinkRoadSegmentToStreetNameRequestHandler Handler { get; }

    private static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
    }

    public LinkRoadSegmentToStreetNameRequestHandlerTests(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<LinkRoadSegmentToStreetNameRequestHandler> logger)
    {
        var store = new InMemoryStreamStore();
        var roadRegistryContext = new RoadRegistryContext(
            new EventSourcedEntityMap(),
            store,
            new FakeRoadNetworkSnapshotReader(),
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings(),
            ContextModule.RoadNetworkEventsEventMapping
        );
        var streetNameCache = new FakeStreetNameCache()
            .AddStreetName(WellKnownStreetNameIds.Proposed, "Proposed street", nameof(StreetNameStatus.Proposed))
            .AddStreetName(WellKnownStreetNameIds.Current, "Current street", nameof(StreetNameStatus.Current))
            .AddStreetName(WellKnownStreetNameIds.Retired, "Retired street", nameof(StreetNameStatus.Retired));

        Handler = new LinkRoadSegmentToStreetNameRequestHandler(commandHandlerDispatcher, logger, store, roadRegistryContext, streetNameCache);
        Fixture = (RoadNetworkFixture)new RoadNetworkFixture().WithStore(store);
    }

    private async Task GivenSegment1Added()
    {
        await Fixture.Given(Organizations.ToStreamName(Fixture.ChangedByOrganization), new ImportedOrganization
        {
            Code = Fixture.ChangedByOrganization,
            Name = Fixture.ChangedByOrganizationName,
            When = InstantPattern.ExtendedIso.Format(Fixture.Clock.GetCurrentInstant())
        });
        await Fixture.Given(RoadNetworks.Stream, new RoadNetworkChangesAccepted
        {
            RequestId = Fixture.RequestId,
            Reason = Fixture.ReasonForChange,
            Operator = Fixture.ChangedByOperator,
            OrganizationId = Fixture.ChangedByOrganization,
            Organization = Fixture.ChangedByOrganizationName,
            Changes = new[]
            {
                new AcceptedChange
                {
                    RoadNodeAdded = Fixture.StartNode1Added
                },
                new AcceptedChange
                {
                    RoadNodeAdded = Fixture.EndNode1Added
                },
                new AcceptedChange
                {
                    RoadSegmentAdded = Fixture.Segment1Added
                }
            },
            When = InstantPattern.ExtendedIso.Format(Fixture.Clock.GetCurrentInstant())
        });
    }

    [Fact]
    public async Task LinkRoadSegmentToStreetName_RoadSegment_NotExists()
    {
        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(int.MaxValue, 99999, 0);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Road segment does not exist.", ex.Errors.Single().ErrorMessage);
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkRoadSegmentToStreetName_LeftStreetName_Proposed_Current(int streetNameId)
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        var request = new LinkRoadSegmentToStreetNameRequest(1, streetNameId, 0);
        await Handler.HandleAsync(request, CancellationToken.None);

        var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

        Assert.Equal(streetNameId, command!.Changes.Single().ModifyRoadSegment.LeftSideStreetNameId);
    }

    [InlineData(WellKnownStreetNameIds.Proposed)]
    [InlineData(WellKnownStreetNameIds.Current)]
    [Theory]
    public async Task LinkRoadSegmentToStreetName_RightStreetName_Proposed_Current(int streetNameId)
    {
        Fixture.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        var request = new LinkRoadSegmentToStreetNameRequest(1, 0, streetNameId);
        await Handler.HandleAsync(request, CancellationToken.None);

        var command = await Fixture.Store.GetLastCommand<ChangeRoadNetwork>();

        Assert.Equal(streetNameId, command!.Changes.Single().ModifyRoadSegment.RightSideStreetNameId);
    }

    [Fact]
    public async Task LinkRoadSegmentToStreetName_LeftStreetName_Retired()
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(1, WellKnownStreetNameIds.Retired, 0);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Street name does not exist or is retired.", ex.Errors.Single().ErrorMessage);
    }


    [Fact]
    public async Task LinkRoadSegmentToStreetName_RightStreetName_Retired()
    {
        Fixture.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(1, 0, WellKnownStreetNameIds.Retired);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Street name does not exist or is retired.", ex.Errors.Single().ErrorMessage);
    }

    [Fact]
    public async Task LinkRoadSegmentToStreetName_LeftStreetName_NotExists()
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(1, 99999, 0);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Street name does not exist or is retired.", ex.Errors.Single().ErrorMessage);
    }


    [Fact]
    public async Task LinkRoadSegmentToStreetName_RightStreetName_NotExists()
    {
        Fixture.Segment1Added.RightSide.StreetNameId = null;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(1, 0, 99999);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Street name does not exist or is retired.", ex.Errors.Single().ErrorMessage);
    }

    [Fact]
    public async Task LinkRoadSegmentToStreetName_LeftStreetName_AlreadyConnected()
    {
        Fixture.Segment1Added.LeftSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(1, WellKnownStreetNameIds.Proposed, 0);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Road segment is connected to a street name on the left side.", ex.Errors.Single().ErrorMessage);
    }
    
    [Fact]
    public async Task LinkRoadSegmentToStreetName_RightStreetName_AlreadyConnected()
    {
        Fixture.Segment1Added.RightSide.StreetNameId = WellKnownStreetNameIds.Proposed;

        await GivenSegment1Added();

        var ex = await Assert.ThrowsAsync<ValidationException>(() =>
        {
            var request = new LinkRoadSegmentToStreetNameRequest(1, 0, WellKnownStreetNameIds.Proposed);
            return Handler.HandleAsync(request, CancellationToken.None);
        });
        Assert.Equal("Road segment is connected to a street name on the right side.", ex.Errors.Single().ErrorMessage);
    }
}
