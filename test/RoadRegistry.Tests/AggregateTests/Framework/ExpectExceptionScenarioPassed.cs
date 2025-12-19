namespace RoadRegistry.Tests.AggregateTests.Framework;

public class ExpectExceptionScenarioPassed
{
    public ExpectExceptionScenarioPassed(ExpectExceptionScenario scenario)
    {
        Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
    }

    public ExpectExceptionScenario Scenario { get; }
}