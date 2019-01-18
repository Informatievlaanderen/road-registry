﻿namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System;

    public class ScenarioExpectedEventsButRecordedOtherEvents
    {
        public ScenarioExpectedEventsButRecordedOtherEvents(ExpectEventsScenario scenario, RecordedEvent[] actual)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            Actual = actual ?? throw new ArgumentNullException(nameof(actual));
        }

        public ExpectEventsScenario Scenario { get; }
        public RecordedEvent[] Actual { get; }
    }
}
