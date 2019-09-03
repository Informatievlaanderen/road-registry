﻿namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System;

    public class ExpectEventsScenario
    {
        public RecordedEvent[] Givens { get; }
        public Command When { get; }
        public RecordedEvent[] Thens { get; }

        public ExpectEventsScenario(
            RecordedEvent[] givens,
            Command when,
            RecordedEvent[] thens)
        {
            Givens = givens ?? throw new ArgumentNullException(nameof(givens));
            When = when ?? throw new ArgumentNullException(nameof(when));
            Thens = thens ?? throw new ArgumentNullException(nameof(thens));
        }

        public ExpectEventsScenarioPassed Pass()
        {
            return new ExpectEventsScenarioPassed(this);
        }

        public ScenarioExpectedEventsButThrewException ButThrewException(Exception threw)
        {
            return new ScenarioExpectedEventsButThrewException(this, threw);
        }

        public ScenarioExpectedEventsButRecordedOtherEvents ButRecordedOtherEvents(RecordedEvent[] events)
        {
            return new ScenarioExpectedEventsButRecordedOtherEvents(this, events);
        }
    }
}
