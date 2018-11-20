namespace RoadRegistry.Model
{
    using System;
    using System.Threading.Tasks;
    using Aiv.Vbr.EventHandling;
    using AutoFixture;
    using Framework;
    using Newtonsoft.Json;
    using Messages;
    using NodaTime;
    using NodaTime.Testing;
    using SqlStreamStore;
    using Testing;

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

        protected RoadRegistryFixture()
        {
            Fixture = new Fixture();
            Store = new InMemoryStreamStore();
            Clock = new FakeClock(NodaConstants.UnixEpoch);

            _runner = new ScenarioRunner(
                Resolve.WhenEqualToMessage(new RoadNetworkCommandHandlerModule(Store, Clock)),
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
