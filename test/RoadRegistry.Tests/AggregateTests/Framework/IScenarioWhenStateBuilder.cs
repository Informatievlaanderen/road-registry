namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public interface IScenarioWhenStateBuilder
{
    IScenarioThenStateBuilder Then(object[] events);
    IScenarioThenStateBuilder Then(Action<RoadNetworkChangeResult, object[]> assert);
    IScenarioThenNoneStateBuilder ThenNone();
    IScenarioThrowsStateBuilder ThenException(Exception exception);
    IScenarioThrowsStateBuilder ThenException(Func<Exception, bool> exceptionIsAcceptable);
}
