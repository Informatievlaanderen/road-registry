namespace RoadRegistry.Framework.Reactions;

using System.Collections.Generic;

public interface IReactionScenarioGivenStateBuilder
{
    IReactionScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IReactionScenarioThenNoneStateBuilder ThenNone();
}
