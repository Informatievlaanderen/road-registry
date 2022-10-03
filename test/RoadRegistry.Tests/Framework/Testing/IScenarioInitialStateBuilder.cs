namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioInitialStateBuilder
{
    IScenarioGivenNoneStateBuilder GivenNone();
    IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IScenarioWhenStateBuilder When(Command command);
}
