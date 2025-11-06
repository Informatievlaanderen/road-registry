namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;

public class ExpectExceptionScenario
{
    public ExpectExceptionScenario(
        Action<RoadNetworkBuilder>[] givens,
        Command when,
        Exception throws)
    {
        Givens = givens.ThrowIfNull();
        When = when.ThrowIfNull();
        Throws = throws.ThrowIfNull();
    }

    public Action<RoadNetworkBuilder>[] Givens { get; }
    public Exception Throws { get; }
    public Command When { get; }

    public ScenarioExpectedExceptionButRecordedEvents ButRecordedEvents(RecordedEvent[] events)
    {
        return new ScenarioExpectedExceptionButRecordedEvents(this, events);
    }

    public ScenarioExpectedExceptionButThrewOtherException ButThrewException(Exception threw)
    {
        return new ScenarioExpectedExceptionButThrewOtherException(this, threw);
    }

    public ScenarioExpectedExceptionButThrewNoException ButThrewNoException()
    {
        return new ScenarioExpectedExceptionButThrewNoException(this);
    }

    public ExpectExceptionScenarioPassed Pass()
    {
        return new ExpectExceptionScenarioPassed(this);
    }
}
