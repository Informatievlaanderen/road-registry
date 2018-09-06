namespace RoadRegistry.Model
{
    using System;
    using System.Threading.Tasks;
    using Aiv.Vbr.EventHandling;
    using AutoFixture;
    using Events;
    using Framework;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using Testing;

    public abstract class RoadRegistryFixture : IDisposable
    {
        private static readonly JsonSerializerSettings Settings =
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
        private static readonly EventMapping Mapping =
            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

        private readonly InMemoryStreamStore _store;
        private readonly ScenarioRunner _runner;

        protected Fixture Fixture { get; }

        protected RoadRegistryFixture()
        {
            Fixture = new ScenarioFixture();
            _store = new InMemoryStreamStore();
            _runner = new ScenarioRunner(
                Resolve.WhenEqualToMessage(new RoadNetworkCommandHandlerModule(_store)),
                _store,
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
            _store?.Dispose();
        }
    }
}
