namespace RoadRegistry.BackOffice.Framework.Reactions
{
    using System.Collections.Generic;
    using Testing;

    public interface IReactionScenarioThenStateBuilder : IReactionScenarioBuilder
    {
        IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    }
}