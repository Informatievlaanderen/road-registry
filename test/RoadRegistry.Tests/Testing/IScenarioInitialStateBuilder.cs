namespace RoadRegistry.Testing
{
    using System.Collections.Generic;
    using Framework;

    public interface IScenarioInitialStateBuilder
    {
        IScenarioGivenNoneStateBuilder GivenNone();
        IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
        IScenarioWhenStateBuilder When(Message command);
    }
}
