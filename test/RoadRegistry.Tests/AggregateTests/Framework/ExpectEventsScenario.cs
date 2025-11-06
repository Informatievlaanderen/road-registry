namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;

public class ExpectEventsScenario
{
    public ExpectEventsScenario(
        Action<RoadNetworkBuilder>[] givens,
        Command when,
        object[] thens)
    {
        Givens = givens.ThrowIfNull();
        When = when.ThrowIfNull();
        Thens = thens.ThrowIfNull();
    }

    public Action<RoadNetworkBuilder>[] Givens { get; }
    public object[] Thens { get; }
    public Command When { get; }

    public ScenarioExpectedEventsButRecordedOtherEvents ButRecordedOtherEvents(object[] events)
    {
        return new ScenarioExpectedEventsButRecordedOtherEvents(this, events);
    }

    public ScenarioExpectedEventsButThrewException ButThrewException(Exception threw)
    {
        return new ScenarioExpectedEventsButThrewException(this, threw);
    }

    public ExpectEventsScenarioPassed Pass()
    {
        return new ExpectEventsScenarioPassed(this);
    }
}
