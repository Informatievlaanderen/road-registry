namespace RoadRegistry.Tests.AggregateTests.Framework;

using FluentValidation.Results;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadNetwork;
using Problems = ValueObjects.Problems.Problems;

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
            MaxStructDepth = 5,
            CustomComparers =
            [
                new ProblemsComparer(RootComparerFactory.GetRootComparer())
            ]
        };
        SerializerSettings = CreateSerializerSettings();
    }

    public async Task<object> RunAsync(ExpectEventsScenario scenario)
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

            var roadNetworkChangeResult = roadNetwork.Change(roadNetworkChanges, null, _roadNetworkIdGenerator);
            if (roadNetworkChangeResult.Problems.HasError())
            {
                if (scenario.Assert is not null)
                {
                    scenario.Assert(roadNetworkChangeResult, []);
                    return scenario.Pass();
                }

                throw new RoadRegistryProblemsException(roadNetworkChangeResult.Problems);
            }

            var recordedEvents = Enumerable.Empty<object>()
                .Concat(roadNetwork.RoadNodes.SelectMany(x => x.Value.GetChanges()))
                .Concat(roadNetwork.RoadSegments.SelectMany(x => x.Value.GetChanges()))
                .Concat(roadNetwork.GradeSeparatedJunctions.SelectMany(x => x.Value.GetChanges()))
                .Except(givenEvents)
                .ToArray();

            if (scenario.Assert is not null)
            {
                var allRecordedEvents = roadNetwork.GetChanges()
                    .Concat(recordedEvents)
                    .ToArray();
                scenario.Assert(roadNetworkChangeResult, allRecordedEvents);
                return scenario.Pass();
            }

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

    public async Task<object> RunAsync(ExpectExceptionScenario scenario)
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

            var roadNetworkChangeResult = roadNetwork.Change(roadNetworkChanges, null, _roadNetworkIdGenerator);
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

            if (recordedEvents.Length != 0)
            {
                return scenario.ButRecordedEvents(recordedEvents);
            }

            return scenario.ButThrewNoException();
        }
        catch (Exception exception)
        {
            if (scenario.ThrownIsAcceptable is not null)
            {
                if (scenario.ThrownIsAcceptable(exception))
                {
                    return scenario.Pass();
                }

                return scenario.ButThrewException(exception);
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
                CustomComparers =
                [
                    new ValidationFailureComparer(RootComparerFactory.GetRootComparer()),
                    new ProblemsComparer(RootComparerFactory.GetRootComparer())
                ]
            };
            var comparer = new CompareLogic(config);
            var result = comparer.Compare(scenario.Throws, exception);
            if (result.AreEqual)
            {
                return scenario.Pass();
            }

            return scenario.ButThrewException(exception);
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

    private class ProblemsComparer : BaseTypeComparer
    {
        public ProblemsComparer(RootComparer comparer)
            : base(comparer)
        {
        }

        public override void CompareType(CompareParms parms)
        {
            var left = ((Problems)parms.Object1).ToList();
            var right = ((Problems)parms.Object2).ToList();

            if (left.Count != right.Count)
            {
                AddDifference(parms);
                return;
            }

            for (var i = 0; i < left.Count; i++)
            {
                var leftProblem = left[i];
                var rightProblem = right[i];

                if (!leftProblem.Equals(rightProblem))
                {
                    var difference = new Difference
                    {
                        Object1 = leftProblem,
                        Object1TypeName = leftProblem.GetType().Name,
                        Object1Value = leftProblem.ToString(),
                        Object2 = rightProblem,
                        Object2TypeName = rightProblem.GetType().Name,
                        Object2Value = rightProblem.ToString(),
                        ParentObject1 = parms.ParentObject1,
                        ParentObject2 = parms.ParentObject2
                    };
                    parms.Result.Differences.Add(difference);
                }
            }
        }

        public override bool IsTypeMatch(Type type1, Type type2)
        {
            return type1 == typeof(Problems) && type2 == typeof(Problems);
        }
    }
}
