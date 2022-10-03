namespace RoadRegistry.Tests.Framework.Reactions;

public interface IReactionScenarioGivenNoneStateBuilder
{
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IReactionScenarioThenNoneStateBuilder ThenNone();
}
