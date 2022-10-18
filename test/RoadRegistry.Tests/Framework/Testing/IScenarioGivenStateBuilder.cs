namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public interface IScenarioGivenStateBuilder
{
    IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
    IScenarioWhenStateBuilder When(Command command);
}