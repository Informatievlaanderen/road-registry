namespace RoadRegistry.Tests.AggregateTests.Framework;

using Extensions;
using RoadRegistry.BackOffice.Framework;

public class ExpectExceptionScenario
{
    public ExpectExceptionScenario(
        Action<RoadNetworkBuilder>[] givens,
        Command when,
        Exception? throws,
        Func<Exception, bool>? thrownIsAcceptable)
    {
        Givens = givens.ThrowIfNull();
        When = when.ThrowIfNull();
        Throws = throws;
        ThrownIsAcceptable = thrownIsAcceptable;
    }

    public Action<RoadNetworkBuilder>[] Givens { get; }
    public Command When { get; }
    public Exception? Throws { get; }
    public Func<Exception, bool>? ThrownIsAcceptable { get; }

    public ScenarioExpectedExceptionButRecordedEvents ButRecordedEvents(object[] events)
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
