namespace RoadRegistry.Tests.Framework.Testing;

using Be.Vlaanderen.Basisregisters.EventHandling;
using FluentValidation.Results;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Framework;
using SqlStreamStore;

public class ScenarioRunner
{
    private readonly StreamNameConverter _converter;
    private readonly EventMapping _mapping;
    private readonly CommandHandlerResolver _resolver;
    private readonly JsonSerializerSettings _settings;
    private readonly IStreamStore _store;

    public ScenarioRunner(CommandHandlerResolver resolver, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping, StreamNameConverter converter)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        _converter = converter ?? throw new ArgumentNullException(nameof(converter));
    }

    public ComparisonConfig ComparisonConfig { get; set; }

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

    public async Task<object> RunAsync(ExpectEventsScenario scenario, CancellationToken ct = default)
    {
        var checkpoint = await WriteGivens(scenario.Givens);
        var exception = await Catch.Exception(() => _resolver(scenario.When)(scenario.When, null, ct));
        if (exception != null)
        {
            return scenario.ButThrewException(exception);
        }

        var recordedEvents = await ReadThens(checkpoint);
        var config = ComparisonConfig ?? new ComparisonConfig
        {
            MaxDifferences = int.MaxValue,
            MaxStructDepth = 5
        };
        var comparer = new CompareLogic(config);
        var expectedEvents = Array.ConvertAll(scenario.Thens,
            then => new RecordedEvent(_converter(new StreamName(then.Stream)), then.Event));
        var result = comparer.Compare(expectedEvents, recordedEvents);
        if (result.AreEqual)
        {
            return scenario.Pass();
        }

        return scenario.ButRecordedOtherEvents(recordedEvents);
    }

    public async Task<object> RunAsync(ExpectExceptionScenario scenario, CancellationToken ct = default)
    {
        var checkpoint = await WriteGivens(scenario.Givens);

        var exception = await Catch.Exception(() => _resolver(scenario.When)(scenario.When, null, ct));
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
                //new PointMComparer(RootComparerFactory.GetRootComparer())
            }
        };
        var comparer = new CompareLogic(config);
        var result = comparer.Compare(scenario.Throws, exception);
        if (result.AreEqual)
        {
            return scenario.Pass();
        }

        return scenario.ButThrewException(exception);
    }

    //        private class PointMComparer : BaseTypeComparer
    //        {
    //            public PointMComparer(RootComparer comparer)
    //                :base(comparer)
    //            {
    //            }
    //
    //            public override void CompareType(CompareParms parms)
    //            {
    //                var left = (PointM)parms.Object1;
    //                var right = (PointM)parms.Object2;
    //                if(!Equals(left.X, right.X)
    //                   || !Equals(left.Y, right.Y)
    //                   || !Equals(left.Z, right.Z)
    //                   || !Equals(left.M, right.M))
    //                {
    //                    var difference = new Difference
    //                    {
    //                        Object1 = left,
    //                        Object1TypeName = left.GetType().Name,
    //                        Object1Value = left.ToString(),
    //                        Object2 = right,
    //                        Object2TypeName = right.GetType().Name,
    //                        Object2Value = right.ToString(),
    //                        ParentObject1 = parms.ParentObject1,
    //                        ParentObject2 = parms.ParentObject2
    //                    };
    //                    parms.Result.Differences.Add(difference);
    //                }
    //            }
    //
    //            public override bool IsTypeMatch(Type type1, Type type2)
    //            {
    //                return type1 == typeof(PointM) && type2 == typeof(PointM);
    //            }
    //        }

    public Task<long> WriteGivens(RecordedEvent[] givens)
    {
        return _store.Given(_mapping, _settings, _converter, givens);
    }

    private class ValidationFailureComparer : BaseTypeComparer
    {
        public ValidationFailureComparer(RootComparer comparer)
            : base(comparer)
        {
        }

        public override void CompareType(CompareParms parms)
        {
            var left = (ValidationFailure)parms.Object1;
            var right = (ValidationFailure)parms.Object2;
            if (!Equals(left.PropertyName, right.PropertyName)
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
}
