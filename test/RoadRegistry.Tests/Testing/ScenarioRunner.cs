namespace RoadRegistry.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Framework;
    using KellermanSoftware.CompareNetObjects;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class ScenarioRunner
    {
        private readonly CommandHandlerResolver _resolver;
        private readonly IStreamStore _store;
        private readonly JsonSerializerSettings _settings;
        private readonly StreamNameConverter _converter;

        public ScenarioRunner(CommandHandlerResolver resolver, IStreamStore store, JsonSerializerSettings settings, StreamNameConverter converter)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _converter = converter ?? throw new ArgumentNullException(nameof(converter));
        }

        public async Task<object> RunAsync(ExpectEventsScenario scenario, CancellationToken ct = default)
        {
            var checkpoint = await WriteGivens(scenario.Givens);
            var exception = await Catch.Exception(() => _resolver(scenario.When).Handler(scenario.When, ct));
            if (exception != null)
            {
                return scenario.ButThrewException(exception);
            }

            var recordedEvents = await ReadThens(checkpoint);
            if (scenario.Givens.Length != 0 && recordedEvents.Length != 0)
            {
                recordedEvents = recordedEvents.Skip(1).ToArray();
            }
            var config = new ComparisonConfig
            {
                MaxDifferences = int.MaxValue,
                MaxStructDepth = 5
            };
            var comparer = new CompareLogic(config);
            var expectedEvents = Array.ConvertAll(scenario.Thens,
                then => new RecordedEvent(_converter(new StreamName(then.Stream)), then.Event));
            var result = comparer.Compare(expectedEvents, recordedEvents);
            if (result.AreEqual) return scenario.Pass();
            return scenario.ButRecordedOtherEvents(recordedEvents);
        }

        public async Task<object> RunAsync(ExpectExceptionScenario scenario, CancellationToken ct = default)
        {
            var checkpoint = await WriteGivens(scenario.Givens);

            var exception = await Catch.Exception(() => _resolver(scenario.When).Handler(scenario.When, ct));
            if (exception == null)
            {
                var recordedEvents = await ReadThens(checkpoint);
                if (scenario.Givens.Length != 0 && recordedEvents.Length != 0)
                {
                    recordedEvents = recordedEvents.Skip(1).ToArray();
                }
                if (recordedEvents.Length != 0)
                {
                    return scenario.ButRecordedEvents(recordedEvents);
                }
                return scenario.ButThrewNoException();
            }

            var config = new ComparisonConfig
            {
                MaxDifferences = int.MaxValue,
                MaxStructDepth = 5,
                MembersToIgnore =
                {
                    "StackTrace",
                    "Source",
                    "TargetSite"
                }
            };
            var comparer = new CompareLogic(config);
            var result = comparer.Compare(scenario.Throws, exception);
            if (result.AreEqual) return scenario.Pass();
            return scenario.ButThrewException(exception);
        }

        private async Task<long> WriteGivens(RecordedEvent[] givens)
        {
            var checkpoint = Position.Start;
            foreach (var stream in givens.GroupBy(given => given.Stream))
            {
                var result = await _store.AppendToStream(
                    _converter(new StreamName(stream.Key)).ToString(),
                    ExpectedVersion.NoStream,
                    stream.Select(given => new NewStreamMessage(
                        Guid.NewGuid(),
                        given.Event.GetType().FullName,
                        JsonConvert.SerializeObject(given.Event, _settings)
                    )).ToArray());
                checkpoint = result.CurrentPosition;
            }
            return checkpoint;
        }

        private async Task<RecordedEvent[]> ReadThens(long position)
        {
            var recorded = new List<RecordedEvent>();
            var page = await _store.ReadAllForwards(position, 1024);
            foreach (var then in page.Messages.Where(message => message.Position != position))
            {
                recorded.Add(
                    new RecordedEvent(
                    new StreamName(then.StreamId),
                    JsonConvert.DeserializeObject(
                        await then.GetJsonData(),
                        Type.GetType(then.Type, true),
                        _settings
                    )
                )
                );
            }
            while (!page.IsEnd)
            {
                page = await page.ReadNext();
                foreach (var then in page.Messages)
                {
                    recorded.Add(
                        new RecordedEvent(
                        new StreamName(then.StreamId),
                        JsonConvert.DeserializeObject(
                            await then.GetJsonData(),
                            Type.GetType(then.Type, true),
                            _settings
                        )
                    )
                    );
                }
            }
            return recorded.ToArray();
        }
    }
}
