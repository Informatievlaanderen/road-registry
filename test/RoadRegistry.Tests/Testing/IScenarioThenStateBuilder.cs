namespace RoadRegistry.Testing
{
    using System.Collections.Generic;

    public interface IScenarioThenStateBuilder : IExpectEventsScenarioBuilder
    {
        IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    }
}