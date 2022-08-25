namespace RoadRegistry.Framework.Testing;

using BackOffice.Framework;

public interface IScenarioThenStateBuilder : IExpectEventsScenarioBuilder
{
    IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
}
