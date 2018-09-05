namespace RoadRegistry.Testing
{
    using System;
    using System.Collections.Generic;

    public interface IScenarioWhenStateBuilder
    {
        IScenarioThenNoneStateBuilder ThenNone();
        IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
        IScenarioThrowsStateBuilder Throws(Exception exception);
    }
}