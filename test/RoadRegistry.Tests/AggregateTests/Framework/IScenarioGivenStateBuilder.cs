namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;
using RoadRegistry.RoadNetwork;

public interface IScenarioGivenStateBuilder
{
    IScenarioGivenStateBuilder Given(RoadNetwork given);
    IScenarioWhenStateBuilder When(Command command);
}
