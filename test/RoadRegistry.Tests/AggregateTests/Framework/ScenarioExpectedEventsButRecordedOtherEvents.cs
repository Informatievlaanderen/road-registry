namespace RoadRegistry.Tests.AggregateTests.Framework;

public class ScenarioExpectedEventsButRecordedOtherEvents
{
    public ScenarioExpectedEventsButRecordedOtherEvents(ExpectEventsScenario scenario, object[] actual)
    {
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        Actual = actual ?? throw new ArgumentNullException(nameof(actual));
    }

    public object[] Actual { get; }
    public ExpectEventsScenario Scenario { get; }
}
