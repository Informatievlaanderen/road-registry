namespace RoadRegistry.BackOffice.Framework.Reactions
{
    using System.Collections.Generic;
    using Testing;

    public interface IReactionScenarioGivenStateBuilder
    {
        IReactionScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
        IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
        IReactionScenarioThenNoneStateBuilder ThenNone();
    }
}