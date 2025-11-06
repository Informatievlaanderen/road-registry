namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;
using RoadRegistry.RoadNetwork;

public class Scenario : IScenarioInitialStateBuilder
{
    private static readonly IScenarioInitialStateBuilder Builder =
        new ScenarioBuilder(
            [],
            null,
            [],
            null);

    public IScenarioGivenStateBuilder Given(Action<RoadNetworkBuilder> given)
    {
        return Builder.Given(given);
    }
    public IScenarioGivenNoneStateBuilder GivenNone()
    {
        return Builder.GivenNone();
    }

    public IScenarioWhenStateBuilder When(Command command)
    {
        return Builder.When(command);
    }

    private sealed class ScenarioBuilder :
        IScenarioInitialStateBuilder,
        IScenarioGivenNoneStateBuilder,
        IScenarioGivenStateBuilder,
        IScenarioWhenStateBuilder,
        IScenarioThenNoneStateBuilder,
        IScenarioThenStateBuilder,
        IScenarioThrowsStateBuilder
    {
        private readonly Action<RoadNetworkBuilder>[] _givens;
        private readonly object[] _thens;
        private readonly Command _when;
        private readonly Exception _throws;

        public ScenarioBuilder(Action<RoadNetworkBuilder>[] givens, Command when, object[] thens, Exception throws)
        {
            _givens = givens;
            _when = when;
            _thens = thens;
            _throws = throws;
        }

        ExpectEventsScenario IExpectEventsScenarioBuilder.Build()
        {
            return new ExpectEventsScenario(_givens, _when, _thens);
        }

        ExpectExceptionScenario IExpectExceptionScenarioBuilder.Build()
        {
            return new ExpectExceptionScenario(_givens, _when, _throws);
        }

        public IScenarioGivenStateBuilder Given(Action<RoadNetworkBuilder> given)
        {
            ArgumentNullException.ThrowIfNull(given);

            return new ScenarioBuilder(_givens.Concat([given]).ToArray(), _when, _thens, _throws);
        }

        IScenarioGivenNoneStateBuilder IScenarioInitialStateBuilder.GivenNone()
        {
            return this;
        }

        IScenarioThenStateBuilder IScenarioThenStateBuilder.Then(object[] events)
        {
            return new ScenarioBuilder(_givens, _when, _thens.Concat(events).ToArray(), _throws);
        }

        IScenarioThenStateBuilder IScenarioWhenStateBuilder.Then(object[] events)
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

        public IScenarioGivenStateBuilder Given(RoadNetwork given)
        {
            throw new NotImplementedException();
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
            ArgumentNullException.ThrowIfNull(command);

            return new ScenarioBuilder(_givens, command, _thens, _throws);
        }
    }
}
