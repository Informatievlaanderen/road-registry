namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioWhenStateBuilder
{
    IScenarioThenNoneStateBuilder ThenNone();
    IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IScenarioThrowsStateBuilder Throws(Exception exception);
}
