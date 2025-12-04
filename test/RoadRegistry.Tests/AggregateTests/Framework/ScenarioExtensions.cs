namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.RoadNetwork;
using ValueObjects.Problems;

public static class ScenarioExtensions
{
    public static IScenarioGivenStateBuilder Given(this Scenario scenario, Func<RoadNetworkChanges, RoadNetworkChanges> roadNetworkChangesBuilder)
    {
        return scenario.Given(builder => builder.Add(roadNetworkChangesBuilder(RoadNetworkChanges.Start().WithProvenance(new FakeProvenance()))));
    }

    public static IScenarioWhenStateBuilder When(this IScenarioGivenStateBuilder builder, Func<RoadNetworkChanges, RoadNetworkChanges> roadNetworkChangesBuilder)
    {
        return builder.When(new Command(roadNetworkChangesBuilder(RoadNetworkChanges.Start().WithProvenance(new FakeProvenance()))));
    }

    public static IScenarioThrowsStateBuilder ThenProblems(this IScenarioWhenStateBuilder builder, Problem problem)
    {
        return ThenProblems(builder, Problems.Single(problem));
    }
    public static IScenarioThrowsStateBuilder ThenProblems(this IScenarioWhenStateBuilder builder, params Problem[] problems)
    {
        return ThenProblems(builder, Problems.Many(problems));
    }
    public static IScenarioThrowsStateBuilder ThenProblems(this IScenarioWhenStateBuilder builder, Problems problems)
    {
        return builder.ThenException(new RoadRegistryProblemsException(problems));
    }
    public static IScenarioThrowsStateBuilder ThenProblems(this IScenarioWhenStateBuilder builder, Func<Problems, bool> problemsAreAcceptable)
    {
        return builder.ThenException(ex => ex is RoadRegistryProblemsException problemsException && problemsAreAcceptable(problemsException.Problems));
    }

    public static IScenarioThrowsStateBuilder ThenContainsProblems(this IScenarioWhenStateBuilder builder, params Problem[] problems)
    {
        return ThenContainsProblems(builder, Problems.Many(problems));
    }
    public static IScenarioThrowsStateBuilder ThenContainsProblems(this IScenarioWhenStateBuilder builder, Problems problems)
    {
        return builder.ThenException(ex =>
        {
            return ex is RoadRegistryProblemsException problemsException && problems.All(expectedProblem => problemsException.Problems.Contains(expectedProblem));
        });
    }
}
