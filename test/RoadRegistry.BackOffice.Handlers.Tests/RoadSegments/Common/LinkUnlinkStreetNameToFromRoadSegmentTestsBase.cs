namespace RoadRegistry.BackOffice.Handlers.Tests.RoadSegments.Common;

using Abstractions;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore;
using StreetNameConsumer.Schema;
using AcceptedChange = Messages.AcceptedChange;

public abstract class LinkUnlinkStreetNameToFromRoadSegmentTestsBase<TRequest, TResponse, THandler>
    where TRequest : EndpointRequest<TResponse>
    where TResponse : EndpointResponse
    where THandler : EndpointRequestHandler<TRequest, TResponse>
{
    protected LinkUnlinkStreetNameToFromRoadSegmentTestsBase(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<THandler> logger)
    {
        var store = new InMemoryStreamStore();
        RoadRegistryContext = new RoadRegistryContext(
            new EventSourcedEntityMap(),
            store,
            new FakeRoadNetworkSnapshotReader(),
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings(),
            ContextModule.RoadNetworkEventsEventMapping
        );
        StreetNameCache = new FakeStreetNameCache()
            .AddStreetName(WellKnownStreetNameIds.Proposed, "Proposed street", nameof(StreetNameStatus.Proposed))
            .AddStreetName(WellKnownStreetNameIds.Current, "Current street", nameof(StreetNameStatus.Current))
            .AddStreetName(WellKnownStreetNameIds.Retired, "Retired street", nameof(StreetNameStatus.Retired));
        Fixture = (RoadNetworkFixture)new RoadNetworkFixture().WithStore(store);

        Handler = CreateHandler(commandHandlerDispatcher, logger);
    }

    protected RoadRegistryContext RoadRegistryContext { get; }
    protected IStreetNameCache StreetNameCache { get; }
    protected RoadNetworkFixture Fixture { get; }
    protected THandler Handler { get; }
    protected abstract THandler CreateHandler(CommandHandlerDispatcher commandHandlerDispatcher, ILogger<THandler> logger);

    protected async Task GivenSegment1Added()
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

    protected string StreetNamePuri(int identifier)
    {
        return $"https://data.vlaanderen.be/id/straatnaam/{identifier}";
    }

    protected static class WellKnownStreetNameIds
    {
        public const int Proposed = 1;
        public const int Current = 2;
        public const int Retired = 3;
    }
}
