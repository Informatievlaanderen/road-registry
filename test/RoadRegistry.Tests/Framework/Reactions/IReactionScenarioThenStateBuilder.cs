namespace RoadRegistry.Framework.Reactions;

public interface IReactionScenarioThenStateBuilder : IReactionScenarioBuilder
{
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
}
