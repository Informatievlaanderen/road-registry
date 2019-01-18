﻿namespace RoadRegistry.BackOffice.Framework.Testing
{
    using System;

    public class ScenarioExpectedExceptionButThrewNoException
    {
        public ScenarioExpectedExceptionButThrewNoException(ExpectExceptionScenario scenario)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
        }

        public ExpectExceptionScenario Scenario { get; }
    }
}
