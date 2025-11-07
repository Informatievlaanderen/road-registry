namespace RoadRegistry.Tests.AggregateTests.Framework;

public class ScenarioExpectedExceptionButRecordedEvents
{
    public ScenarioExpectedExceptionButRecordedEvents(ExpectExceptionScenario scenario, object[] actual)
    {
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        Actual = actual ?? throw new ArgumentNullException(nameof(actual));
    }

    public object[] Actual { get; }
    public ExpectExceptionScenario Scenario { get; }
}
