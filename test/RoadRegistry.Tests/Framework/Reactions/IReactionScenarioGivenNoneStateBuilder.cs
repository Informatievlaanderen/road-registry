namespace RoadRegistry.Framework.Reactions
{
    using System.Collections.Generic;

    public interface IReactionScenarioGivenNoneStateBuilder
    {
        IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
        IReactionScenarioThenNoneStateBuilder ThenNone();
    }
}
