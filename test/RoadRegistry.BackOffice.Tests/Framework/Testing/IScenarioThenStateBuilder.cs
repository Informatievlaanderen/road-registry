namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System.Collections.Generic;

    public interface IScenarioThenStateBuilder : IExpectEventsScenarioBuilder
    {
        IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    }
}
