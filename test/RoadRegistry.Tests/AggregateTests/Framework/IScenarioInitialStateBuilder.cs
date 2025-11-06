namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioInitialStateBuilder
{
    IScenarioGivenStateBuilder Given(Action<RoadNetworkBuilder> given);
    IScenarioGivenNoneStateBuilder GivenNone();
    IScenarioWhenStateBuilder When(Command command);
}
