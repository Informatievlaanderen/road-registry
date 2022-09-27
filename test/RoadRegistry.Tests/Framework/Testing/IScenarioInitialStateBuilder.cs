namespace RoadRegistry.Framework.Testing;

using BackOffice.Framework;

public interface IScenarioInitialStateBuilder
{
    IScenarioGivenNoneStateBuilder GivenNone();
    IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IScenarioWhenStateBuilder When(Command command);
}
