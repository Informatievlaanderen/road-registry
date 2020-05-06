namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Framework;
    using Messages;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
    using RoadRegistry.Framework.Testing;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public abstract class RoadRegistryFixture : IDisposable
    {
        private static readonly JsonSerializerSettings Settings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly EventMapping Mapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        private readonly ScenarioRunner _runner;

        protected Fixture Fixture { get; }
        protected IStreamStore Store { get; }
        protected FakeClock Clock { get; }

        private class FakeRoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
        {
            public Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
            {
                return Task.FromResult<(RoadNetworkSnapshot snapshot, int version)>((null, ExpectedVersion.NoStream));
            }
        }

        protected RoadRegistryFixture()
        {
            Fixture = new Fixture();
            Store = new InMemoryStreamStore();
            Clock = new FakeClock(NodaConstants.UnixEpoch);

            _runner = new ScenarioRunner(
                Resolve.WhenEqualToMessage(new RoadNetworkCommandModule(Store, new FakeRoadNetworkSnapshotReader(), Clock)),
                Store,
                Settings,
                Mapping,
                StreamNameConversions.PassThru
            );
        }

        protected Task Run(Func<Scenario, IExpectExceptionScenarioBuilder> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder(new Scenario()).AssertAsync(_runner);
        }

        protected Task Run(Func<Scenario, IExpectEventsScenarioBuilder> builder)
        {
            if (builder == null) throw new ArgumentNullException(nameof(builder));
            return builder(new Scenario()).AssertAsync(_runner);
        }

        public void Dispose()
        {
            Store?.Dispose();
        }
    }
}
