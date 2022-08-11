namespace RoadRegistry.Framework.Testing;

using System.Collections.Generic;
using BackOffice.Framework;

public interface IScenarioThenStateBuilder : IExpectEventsScenarioBuilder
{
    IScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
}
