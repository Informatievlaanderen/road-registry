namespace RoadRegistry.Tests.Framework.Reactions;

public interface IReactionScenarioThenStateBuilder : IReactionScenarioBuilder
{
    IReactionScenarioThenStateBuilder Then(IEnumerable<RecordedEvent> events);
}