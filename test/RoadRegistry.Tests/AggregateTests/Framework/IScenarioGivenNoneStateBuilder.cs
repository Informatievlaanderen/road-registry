namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioGivenNoneStateBuilder
{
    IScenarioWhenStateBuilder When(Command command);
}
