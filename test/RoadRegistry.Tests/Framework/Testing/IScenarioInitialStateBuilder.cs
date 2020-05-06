namespace RoadRegistry.Framework.Testing
{
    using System.Collections.Generic;
    using BackOffice.Framework;

    public interface IScenarioInitialStateBuilder
    {
        IScenarioGivenNoneStateBuilder GivenNone();
        IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
        IScenarioWhenStateBuilder When(Command command);
    }
}
