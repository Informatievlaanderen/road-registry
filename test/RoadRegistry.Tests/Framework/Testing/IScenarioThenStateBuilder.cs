namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioThenStateBuilder : IExpectEventsScenarioBuilder
{
    IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
}
