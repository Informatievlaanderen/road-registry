namespace RoadRegistry.Framework.Reactions;

public interface IReactionScenarioGivenNoneStateBuilder
{
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
    IReactionScenarioThenNoneStateBuilder ThenNone();
}
