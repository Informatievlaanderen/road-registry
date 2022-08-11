namespace RoadRegistry.Framework.Reactions;

using System.Collections.Generic;

public interface IReactionScenarioThenStateBuilder : IReactionScenarioBuilder
{
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
}
