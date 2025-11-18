namespace RoadRegistry.Tests.AggregateTests.Framework;

public interface IScenarioWhenStateBuilder
{
    IScenarioThenStateBuilder Then(params object[] events);
    IScenarioThenNoneStateBuilder ThenNone();
    IScenarioThrowsStateBuilder ThenException(Exception exception);
    IScenarioThrowsStateBuilder ThenException(Func<Exception, bool> exceptionIsAcceptable);
}
