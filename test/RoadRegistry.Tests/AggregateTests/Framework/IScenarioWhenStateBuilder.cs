namespace RoadRegistry.Tests.AggregateTests.Framework;

public interface IScenarioWhenStateBuilder
{
    IScenarioThenStateBuilder Then(params object[] events);
    IScenarioThenNoneStateBuilder ThenNone();
    IScenarioThrowsStateBuilder Throws(Exception exception);
}
