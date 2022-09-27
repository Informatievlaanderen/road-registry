namespace RoadRegistry.Framework.Testing;

using BackOffice.Framework;

public interface IScenarioWhenStateBuilder
{
    IScenarioThenNoneStateBuilder ThenNone();
    IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IScenarioThrowsStateBuilder Throws(Exception exception);
}
