namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork;

public interface IScenarioGivenStateBuilder
{
    IScenarioGivenStateBuilder Given(ScopedRoadNetwork given);
    IScenarioWhenStateBuilder When(Command command);
}
