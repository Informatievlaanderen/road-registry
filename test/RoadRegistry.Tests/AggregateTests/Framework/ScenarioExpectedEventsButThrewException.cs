namespace RoadRegistry.Tests.AggregateTests.Framework;

using Tests.Framework.Testing;

public class ScenarioExpectedEventsButThrewException
{
    public ScenarioExpectedEventsButThrewException(ExpectEventsScenario scenario, Exception actual)
    {
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        Actual = actual ?? throw new ArgumentNullException(nameof(actual));
    }

    public Exception Actual { get; }
    public ExpectEventsScenario Scenario { get; }
}