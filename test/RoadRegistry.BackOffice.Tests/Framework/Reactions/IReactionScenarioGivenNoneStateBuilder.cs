namespace RoadRegistry.BackOffice.Framework.Reactions
{
    using System.Collections.Generic;
    using Testing;

    public interface IReactionScenarioGivenNoneStateBuilder
    {
        IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
        IReactionScenarioThenNoneStateBuilder ThenNone();
    }
}