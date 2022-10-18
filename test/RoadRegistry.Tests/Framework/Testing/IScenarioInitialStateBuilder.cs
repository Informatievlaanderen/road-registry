namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioInitialStateBuilder
{
    IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IScenarioGivenNoneStateBuilder GivenNone();
    IScenarioWhenStateBuilder When(Command command);
}
