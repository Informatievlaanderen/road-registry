namespace RoadRegistry.Tests.AggregateTests.Framework;

public class ScenarioExpectedExceptionButThrewOtherException
{
    public ScenarioExpectedExceptionButThrewOtherException(ExpectExceptionScenario scenario, Exception actual)
    {
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        Actual = actual ?? throw new ArgumentNullException(nameof(actual));
    }

    public Exception Actual { get; }
    public ExpectExceptionScenario Scenario { get; }
}
