namespace RoadRegistry.Testing
{
    using System;
    using Framework;

    public class ExpectExceptionScenario
    {
        public RecordedEvent[] Givens { get; }
        public Message When { get; }
        public Exception Throws { get; }

        public ExpectExceptionScenario(
            RecordedEvent[] givens,
            Message when,
            Exception throws)
        {
            Givens = givens ?? throw new ArgumentNullException(nameof(givens));
            When = when ?? throw new ArgumentNullException(nameof(when));
            Throws = throws ?? throw new ArgumentNullException(nameof(throws));
        }

        public ExpectExceptionScenarioPassed Pass()
        {
            return new ExpectExceptionScenarioPassed(this);
        }

        public ScenarioExpectedExceptionButThrewOtherException ButThrewException(Exception threw)
        {
            return new ScenarioExpectedExceptionButThrewOtherException(this, threw);
        }

        public ScenarioExpectedExceptionButThrewNoException ButThrewNoException()
        {
            return new ScenarioExpectedExceptionButThrewNoException(this);
        }

        public ScenarioExpectedExceptionButRecordedEvents ButRecordedEvents(RecordedEvent[] events)
        {
            return new ScenarioExpectedExceptionButRecordedEvents(this, events);
        }
    }
}
