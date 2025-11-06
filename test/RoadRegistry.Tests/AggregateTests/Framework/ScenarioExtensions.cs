namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;
using RoadRegistry.RoadNetwork;

public static class ScenarioExtensions
{
    public static IScenarioGivenStateBuilder Given(this Scenario scenario, Func<RoadNetworkChanges, RoadNetworkChanges> roadNetworkChangesBuilder)
    {
        return scenario.Given(builder => builder.Add(roadNetworkChangesBuilder(RoadNetworkChanges.Start())));
    }

    public static IScenarioWhenStateBuilder When(this IScenarioGivenStateBuilder builder, Func<RoadNetworkChanges, RoadNetworkChanges> roadNetworkChangesBuilder)
    {
        return builder.When(new Command(roadNetworkChangesBuilder(RoadNetworkChanges.Start())));
    }
}
