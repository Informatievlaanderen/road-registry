namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioWhenStateBuilder
{
    IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IScenarioThenNoneStateBuilder ThenNone();
    IScenarioThrowsStateBuilder Throws(Exception exception);
}