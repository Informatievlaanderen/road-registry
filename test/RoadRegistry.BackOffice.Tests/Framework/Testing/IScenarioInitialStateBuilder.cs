namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System.Collections.Generic;

    public interface IScenarioInitialStateBuilder
    {
        IScenarioGivenNoneStateBuilder GivenNone();
        IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
        IScenarioWhenStateBuilder When(Command command);
    }
}
