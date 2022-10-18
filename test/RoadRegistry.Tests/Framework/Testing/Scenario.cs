namespace RoadRegistry.Tests.Framework.Testing;

using RoadRegistry.BackOffice.Framework;

public class Scenario : IScenarioInitialStateBuilder
{
    private static readonly IScenarioInitialStateBuilder Builder =
        new ScenarioBuilder(
            Array.Empty<RecordedEvent>(),
            null,
            Array.Empty<RecordedEvent>(),
            null);

    public IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events)
    {
        return Builder.Given(events);
    }

    public IScenarioGivenNoneStateBuilder GivenNone()
    {
        return Builder.GivenNone();
    }

    private class ScenarioBuilder :
        IScenarioInitialStateBuilder,
        IScenarioGivenNoneStateBuilder,
        IScenarioGivenStateBuilder,
        IScenarioWhenStateBuilder,
        IScenarioThenNoneStateBuilder,
        IScenarioThenStateBuilder,
        IScenarioThrowsStateBuilder
    {
        public ScenarioBuilder(RecordedEvent[] givens, Command when, RecordedEvent[] thens, Exception throws)
        {
            _givens = givens;
            _when = when;
            _thens = thens;
            _throws = throws;
        }

        private readonly RecordedEvent[] _givens;
        private readonly RecordedEvent[] _thens;
        private readonly Exception _throws;
        private readonly Command _when;

        ExpectEventsScenario IExpectEventsScenarioBuilder.Build()
        {
            return new ExpectEventsScenario(_givens, _when, _thens);
        }

        ExpectExceptionScenario IExpectExceptionScenarioBuilder.Build()
        {
            return new ExpectExceptionScenario(_givens, _when, _throws);
        }

        IScenarioGivenStateBuilder IScenarioGivenStateBuilder.Given(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ScenarioBuilder(_givens.Concat(events).ToArray(), _when, _thens, _throws);
        }

        IScenarioGivenStateBuilder IScenarioInitialStateBuilder.Given(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ScenarioBuilder(events.ToArray(), _when, _thens, _throws);
        }

        IScenarioGivenNoneStateBuilder IScenarioInitialStateBuilder.GivenNone()
        {
            return this;
        }

        IScenarioThenStateBuilder IScenarioThenStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            return new ScenarioBuilder(_givens, _when, _thens.Concat(events).ToArray(), _throws);
        }

        IScenarioThenStateBuilder IScenarioWhenStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            return new ScenarioBuilder(_givens, _when, events.ToArray(), _throws);
        }

        IScenarioThenNoneStateBuilder IScenarioWhenStateBuilder.ThenNone()
        {
            return this;
        }

        IScenarioThrowsStateBuilder IScenarioWhenStateBuilder.Throws(Exception exception)
        {
            return new ScenarioBuilder(_givens, _when, _thens, exception);
        }

        IScenarioWhenStateBuilder IScenarioGivenNoneStateBuilder.When(Command command)
        {
            return When(command);
        }

        IScenarioWhenStateBuilder IScenarioGivenStateBuilder.When(Command command)
        {
            return When(command);
        }

        IScenarioWhenStateBuilder IScenarioInitialStateBuilder.When(Command command)
        {
            return When(command);
        }

        private IScenarioWhenStateBuilder When(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return new ScenarioBuilder(_givens, command, _thens, _throws);
        }
    }

    public IScenarioWhenStateBuilder When(Command command)
    {
        return Builder.When(command);
    }
}
