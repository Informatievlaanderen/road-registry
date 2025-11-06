namespace RoadRegistry.Tests.AggregateTests.Framework;

using KellermanSoftware.CompareNetObjects;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadNetwork;
using Tests.Framework.Projections;
using IRoadNetworkIdGenerator = RoadRegistry.BackOffice.Core.IRoadNetworkIdGenerator;

public class ScenarioRunner
{
    public JsonSerializerSettings SerializerSettings { get; }

    private readonly IRoadNetworkIdGenerator _roadNetworkIdGenerator;
    private readonly ComparisonConfig _comparisonConfig;

    public ScenarioRunner(IRoadNetworkIdGenerator roadNetworkIdGenerator)
    {
        _roadNetworkIdGenerator = roadNetworkIdGenerator;
        _comparisonConfig = new ComparisonConfig
        {
            MaxDifferences = int.MaxValue,
            MaxStructDepth = 5
        };
        SerializerSettings = CreateSerializerSettings();
    }

    public async Task<object> RunAsync(ExpectEventsScenario scenario, CancellationToken ct = default)
    {
        var roadNetwork = BuildRoadNetwork(scenario.Givens);
        var roadNetworkChanges = (RoadNetworkChanges)scenario.When.Body;

        try
        {
            var givenEvents = Enumerable.Empty<object>()
                .Concat(roadNetwork.RoadNodes.SelectMany(x => x.Value.GetChanges()))
                .Concat(roadNetwork.RoadSegments.SelectMany(x => x.Value.GetChanges()))
                .Concat(roadNetwork.GradeSeparatedJunctions.SelectMany(x => x.Value.GetChanges()))
                .ToList();

            var roadNetworkChangeResult = roadNetwork.Change(roadNetworkChanges, _roadNetworkIdGenerator);
            if (roadNetworkChangeResult.Problems.HasError())
            {
                throw new RoadRegistryProblemsException(roadNetworkChangeResult.Problems);
            }

            var recordedEvents = Enumerable.Empty<object>()
                .Concat(roadNetwork.RoadNodes.SelectMany(x => x.Value.GetChanges()))
                .Concat(roadNetwork.RoadSegments.SelectMany(x => x.Value.GetChanges()))
                .Concat(roadNetwork.GradeSeparatedJunctions.SelectMany(x => x.Value.GetChanges()))
                .Except(givenEvents)
                .ToArray();

            var expectedEvents = scenario.Thens;
            var result = Compare(expectedEvents, recordedEvents);
            if (result.AreEqual)
            {
                return scenario.Pass();
            }

            return scenario.ButRecordedOtherEvents(recordedEvents);
        }
        catch (Exception ex)
        {
            return scenario.ButThrewException(ex);
        }
    }

    public ComparisonResult Compare(object expected, object actual)
    {
        var comparer = new CompareLogic(_comparisonConfig);
        return comparer.Compare(
            JsonConvert.SerializeObject(expected, SerializerSettings).Split(Environment.NewLine),
            JsonConvert.SerializeObject(actual, SerializerSettings).Split(Environment.NewLine));
    }

    private RoadNetwork BuildRoadNetwork(Action<RoadNetworkBuilder>[] givens)
    {
        var builder = new RoadNetworkBuilder(_roadNetworkIdGenerator);

        foreach (var given in givens)
        {
            given(builder);
        }

        return builder.Build();
    }

    public async Task<object> RunAsync(ExpectExceptionScenario scenario, CancellationToken ct = default)
    {
        throw new NotImplementedException();
        // var checkpoint = WriteGivens(scenario.Givens);
        //
        // var exception = await Catch.Exception(() => _resolver(scenario.When)(scenario.When, null, ct));
        // if (exception == null)
        // {
        //     var recordedEvents = await ReadThens(checkpoint);
        //     if (recordedEvents.Length != 0)
        //     {
        //         return scenario.ButRecordedEvents(recordedEvents);
        //     }
        //
        //     return scenario.ButThrewNoException();
        // }
        //
        // var config = new ComparisonConfig
        // {
        //     MaxDifferences = int.MaxValue,
        //     MaxStructDepth = 5,
        //     MembersToIgnore =
        //     {
        //         "StackTrace",
        //         "Source",
        //         "TargetSite"
        //     },
        //     IgnoreObjectTypes = true,
        //     CustomComparers = new List<BaseTypeComparer>
        //     {
        //         new ValidationFailureComparer(RootComparerFactory.GetRootComparer())
        //     }
        // };
        // var comparer = new CompareLogic(config);
        // var result = comparer.Compare(scenario.Throws, exception);
        // if (result.AreEqual)
        // {
        //     return scenario.Pass();
        // }
        //
        // return scenario.ButThrewException(exception);
    }
    //
    // private async Task<RecordedEvent[]> ReadThens(long position)
    // {
    //     var recorded = new List<RecordedEvent>();
    //     var page = await _store.ReadAllForwards(position, 1024);
    //     foreach (var then in page.Messages)
    //     {
    //         recorded.Add(
    //             new RecordedEvent(
    //                 new StreamName(then.StreamId),
    //                 JsonConvert.DeserializeObject(
    //                     await then.GetJsonData(),
    //                     _mapping.GetEventType(then.Type),
    //                     _settings
    //                 )
    //             )
    //         );
    //     }
    //
    //     while (!page.IsEnd)
    //     {
    //         page = await page.ReadNext();
    //         foreach (var then in page.Messages)
    //         {
    //             recorded.Add(
    //                 new RecordedEvent(
    //                     new StreamName(then.StreamId),
    //                     JsonConvert.DeserializeObject(
    //                         await then.GetJsonData(),
    //                         _mapping.GetEventType(then.Type),
    //                         _settings
    //                     )
    //                 )
    //             );
    //         }
    //     }
    //
    //     return recorded.ToArray();
    // }
    //
    // private class ValidationFailureComparer : BaseTypeComparer
    // {
    //     public ValidationFailureComparer(RootComparer comparer)
    //         : base(comparer)
    //     {
    //     }
    //
    //     public override void CompareType(CompareParms parms)
    //     {
    //         var left = (ValidationFailure)parms.Object1;
    //         var right = (ValidationFailure)parms.Object2;
    //         if (!Equals(left.PropertyName, right.PropertyName)
    //             || !Equals(left.ErrorMessage, right.ErrorMessage))
    //         {
    //             var difference = new Difference
    //             {
    //                 Object1 = left,
    //                 Object1TypeName = left.GetType().Name,
    //                 Object1Value = left.ToString(),
    //                 Object2 = right,
    //                 Object2TypeName = right.GetType().Name,
    //                 Object2Value = right.ToString(),
    //                 ParentObject1 = parms.ParentObject1,
    //                 ParentObject2 = parms.ParentObject2
    //             };
    //             parms.Result.Differences.Add(difference);
    //         }
    //     }
    //
    //     public override bool IsTypeMatch(Type type1, Type type2)
    //     {
    //         return type1 == typeof(ValidationFailure) && type2 == typeof(ValidationFailure);
    //     }
    // }


    private static JsonSerializerSettings CreateSerializerSettings()
    {
        var settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        };

        foreach (var converter in WellKnownJsonConverters.Converters)
        {
            settings.Converters.Add(converter);
        }

        return settings;
    }
}
