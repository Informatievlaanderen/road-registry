namespace RoadRegistry.Tests.Framework.Reactions;

public interface IReactionScenarioGivenStateBuilder
{
    IReactionScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IReactionScenarioThenNoneStateBuilder ThenNone();
}
