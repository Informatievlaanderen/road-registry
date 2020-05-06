namespace RoadRegistry.Framework.Testing
{
    using System;
    using System.Collections.Generic;
    using BackOffice.Framework;

    public interface IScenarioWhenStateBuilder
    {
        IScenarioThenNoneStateBuilder ThenNone();
        IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
        IScenarioThrowsStateBuilder Throws(Exception exception);
    }
}
