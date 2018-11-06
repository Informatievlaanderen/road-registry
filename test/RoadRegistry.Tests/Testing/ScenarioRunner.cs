namespace RoadRegistry.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.EventHandling;
    using Aiv.Vbr.Generators.Guid;
    using FluentValidation.Results;
    using Framework;
    using KellermanSoftware.CompareNetObjects;
    using KellermanSoftware.CompareNetObjects.TypeComparers;
    using Model;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class ScenarioRunner
    {
        private readonly CommandHandlerResolver _resolver;
        private readonly IStreamStore _store;
        private readonly JsonSerializerSettings _settings;
        private readonly EventMapping _mapping;
        private readonly StreamNameConverter _converter;

        public ScenarioRunner(CommandHandlerResolver resolver, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping, StreamNameConverter converter)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
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
                },
                IgnoreObjectTypes = true,
                CustomComparers = new List<BaseTypeComparer>
                {
                    new ValidationFailureComparer(RootComparerFactory.GetRootComparer())
                }
            };
            var comparer = new CompareLogic(config);
            var result = comparer.Compare(scenario.Throws, exception);
            if (result.AreEqual) return scenario.Pass();
            return scenario.ButThrewException(exception);
        }

        private class ValidationFailureComparer : BaseTypeComparer
        {
            public ValidationFailureComparer(RootComparer comparer)
                :base(comparer)
            {
            }

            public override void CompareType(CompareParms parms)
            {
                var left = (ValidationFailure)parms.Object1;
                var right = (ValidationFailure)parms.Object2;
                if(!Equals(left.PropertyName, right.PropertyName) 
                || !Equals(left.ErrorMessage, right.ErrorMessage))
                {
                    var difference = new Difference
                    {
                        Object1 = left,
                        Object1TypeName = left.GetType().Name,
                        Object1Value = left.ToString(),
                        Object2 = right,
                        Object2TypeName = right.GetType().Name,
                        Object2Value = right.ToString(),
                        ParentObject1 = parms.ParentObject1,
                        ParentObject2 = parms.ParentObject2
                    };
                    parms.Result.Differences.Add(difference);
                }
            }

            public override bool IsTypeMatch(Type type1, Type type2)
            {
                return type1 == typeof(ValidationFailure) && type2 == typeof(ValidationFailure);
            }
        }

        private async Task<long> WriteGivens(RecordedEvent[] givens)
        {
            var checkpoint = SqlStreamStore.Streams.Position.Start;
            foreach (var stream in givens.GroupBy(given => given.Stream))
            {
                var result = await _store.AppendToStream(
                    _converter(new StreamName(stream.Key)).ToString(),
                    ExpectedVersion.NoStream,
                    stream.Select((given, index) => new NewStreamMessage(
                        Deterministic.Create(Deterministic.Namespaces.Events,
                            $"{given.Stream}-{index}"),
                        _mapping.GetEventName(given.Event.GetType()),
                        JsonConvert.SerializeObject(given.Event, _settings)
                    )).ToArray());
                checkpoint = result.CurrentPosition + 1;
            }
            return checkpoint;
        }

        private async Task<RecordedEvent[]> ReadThens(long position)
        {
            var recorded = new List<RecordedEvent>();
            var page = await _store.ReadAllForwards(position, 1024);
            foreach (var then in page.Messages)
            {
                recorded.Add(
                    new RecordedEvent(
                        new StreamName(then.StreamId),
                        JsonConvert.DeserializeObject(
                            await then.GetJsonData(),
                            _mapping.GetEventType(then.Type),
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
                                _mapping.GetEventType(then.Type),
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
