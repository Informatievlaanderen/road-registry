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

    public IScenarioGivenNoneStateBuilder GivenNone()
    {
        return Builder.GivenNone();
    }

    public IScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events)
    {
        return Builder.Given(events);
    }

    public IScenarioWhenStateBuilder When(Command command)
    {
        return Builder.When(command);
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
        private readonly RecordedEvent[] _givens;
        private readonly RecordedEvent[] _thens;
        private readonly Exception _throws;
        private readonly Command _when;

        public ScenarioBuilder(RecordedEvent[] givens, Command when, RecordedEvent[] thens, Exception throws)
        {
            _givens = givens;
            _when = when;
            _thens = thens;
            _throws = throws;
        }

        IScenarioWhenStateBuilder IScenarioGivenNoneStateBuilder.When(Command command)
        {
            return When(command);
        }

        IScenarioGivenStateBuilder IScenarioGivenStateBuilder.Given(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ScenarioBuilder(_givens.Concat(events).ToArray(), _when, _thens, _throws);
        }

        IScenarioWhenStateBuilder IScenarioGivenStateBuilder.When(Command command)
        {
            return When(command);
        }

        IScenarioGivenNoneStateBuilder IScenarioInitialStateBuilder.GivenNone()
        {
            return this;
        }

        IScenarioGivenStateBuilder IScenarioInitialStateBuilder.Given(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ScenarioBuilder(events.ToArray(), _when, _thens, _throws);
        }

        IScenarioWhenStateBuilder IScenarioInitialStateBuilder.When(Command command)
        {
            return When(command);
        }

        ExpectEventsScenario IExpectEventsScenarioBuilder.Build()
        {
            return new ExpectEventsScenario(_givens, _when, _thens);
        }

        IScenarioThenStateBuilder IScenarioThenStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            return new ScenarioBuilder(_givens, _when, _thens.Concat(events).ToArray(), _throws);
        }

        ExpectExceptionScenario IExpectExceptionScenarioBuilder.Build()
        {
            return new ExpectExceptionScenario(_givens, _when, _throws);
        }

        IScenarioThenNoneStateBuilder IScenarioWhenStateBuilder.ThenNone()
        {
            return this;
        }

        IScenarioThenStateBuilder IScenarioWhenStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            return new ScenarioBuilder(_givens, _when, events.ToArray(), _throws);
        }

        IScenarioThrowsStateBuilder IScenarioWhenStateBuilder.Throws(Exception exception)
        {
            return new ScenarioBuilder(_givens, _when, _thens, exception);
        }

        private IScenarioWhenStateBuilder When(Command command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));
            return new ScenarioBuilder(_givens, command, _thens, _throws);
        }
    }
}
