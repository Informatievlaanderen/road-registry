namespace RoadRegistry.Tests.AggregateTests.Framework;

public interface IScenarioThenStateBuilder : IExpectEventsScenarioBuilder
{
    IScenarioThenStateBuilder Then(object[] events);
}
