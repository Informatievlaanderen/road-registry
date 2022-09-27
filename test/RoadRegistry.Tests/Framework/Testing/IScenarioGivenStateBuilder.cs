namespace RoadRegistry.Framework.Testing;

using BackOffice.Framework;

public interface IScenarioGivenStateBuilder
{
    IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IScenarioWhenStateBuilder When(Command command);
}
