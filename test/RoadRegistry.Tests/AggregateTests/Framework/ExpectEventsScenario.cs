namespace RoadRegistry.Tests.AggregateTests.Framework;

using Extensions;
using RoadNetwork;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork.ValueObjects;

public class ExpectEventsScenario
{
    public ExpectEventsScenario(
        Action<RoadNetworkBuilder>[] givens,
        Command when,
        object[] thens,
        Action<RoadNetworkChangeResult, object[]>? assert)
    {
        Givens = givens.ThrowIfNull();
        When = when.ThrowIfNull();
        Thens = thens.ThrowIfNull();
        Assert = assert;
    }

    public Action<RoadNetworkBuilder>[] Givens { get; }
    public Command When { get; }
    public object[] Thens { get; }
    public Action<RoadNetworkChangeResult, object[]>? Assert { get; }

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
