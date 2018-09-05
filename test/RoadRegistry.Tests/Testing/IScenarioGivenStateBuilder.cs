namespace RoadRegistry.Testing
{
    using System.Collections.Generic;
    using Framework;

    public interface IScenarioGivenStateBuilder
    {
        IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
        IScenarioWhenStateBuilder When(Message command);
    }
}
