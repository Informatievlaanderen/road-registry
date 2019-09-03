namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System.Collections.Generic;

    public interface IScenarioGivenStateBuilder
    {
        IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events);
        IScenarioWhenStateBuilder When(Command command);
    }
}
