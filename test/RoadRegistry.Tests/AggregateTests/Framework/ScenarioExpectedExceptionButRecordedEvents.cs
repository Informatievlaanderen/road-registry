namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;
using Tests.Framework.Testing;

public class ScenarioExpectedExceptionButRecordedEvents
{
    public ScenarioExpectedExceptionButRecordedEvents(ExpectExceptionScenario scenario, RecordedEvent[] actual)
    {
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        Actual = actual ?? throw new ArgumentNullException(nameof(actual));
    }

    public RecordedEvent[] Actual { get; }
    public ExpectExceptionScenario Scenario { get; }
}