namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public class ExpectEventsScenario
{
    public ExpectEventsScenario(
        RecordedEvent[] givens,
        Command when,
        RecordedEvent[] thens)
    {
        Givens = givens ?? throw new ArgumentNullException(nameof(givens));
        When = when ?? throw new ArgumentNullException(nameof(when));
        Thens = thens ?? throw new ArgumentNullException(nameof(thens));
    }

    public RecordedEvent[] Givens { get; }
    public RecordedEvent[] Thens { get; }
    public Command When { get; }

    public ScenarioExpectedEventsButRecordedOtherEvents ButRecordedOtherEvents(RecordedEvent[] events)
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