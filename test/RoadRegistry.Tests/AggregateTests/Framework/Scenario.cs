namespace RoadRegistry.Tests.AggregateTests.Framework;

using RoadRegistry.BackOffice.Framework;
using RoadNetwork = RoadNetwork.RoadNetwork;

public class Scenario : IScenarioInitialStateBuilder
{
    private static readonly IScenarioInitialStateBuilder Builder =
        new ScenarioBuilder(
            [],
            null,
            [],
            null,
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
        private readonly Command _when;
        private readonly object[] _thens;
        private readonly Exception? _throws;
        private readonly Func<Exception, bool>? _thrownIsAcceptable;

        public ScenarioBuilder(Action<RoadNetworkBuilder>[] givens, Command when, object[] thens, Exception? throws, Func<Exception, bool>? thrownIsAcceptable)
        {
            _givens = givens;
            _when = when;
            _thens = thens;
            _throws = throws;
            _thrownIsAcceptable = thrownIsAcceptable;
        }

        ExpectEventsScenario IExpectEventsScenarioBuilder.Build()
        {
            return new ExpectEventsScenario(_givens, _when, _thens);
        }

        ExpectExceptionScenario IExpectExceptionScenarioBuilder.Build()
        {
            return new ExpectExceptionScenario(_givens, _when, _throws, _thrownIsAcceptable);
        }

        public IScenarioGivenStateBuilder Given(Action<RoadNetworkBuilder> given)
        {
            ArgumentNullException.ThrowIfNull(given);

            return new ScenarioBuilder(_givens.Concat([given]).ToArray(), _when, _thens, _throws, _thrownIsAcceptable);
        }

        IScenarioGivenNoneStateBuilder IScenarioInitialStateBuilder.GivenNone()
        {
            return this;
        }

        IScenarioThenStateBuilder IScenarioThenStateBuilder.Then(object[] events)
        {
            return new ScenarioBuilder(_givens, _when, _thens.Concat(events).ToArray(), _throws, _thrownIsAcceptable);
        }

        IScenarioThenStateBuilder IScenarioWhenStateBuilder.Then(object[] events)
        {
            return new ScenarioBuilder(_givens, _when, events.ToArray(), _throws, _thrownIsAcceptable);
        }

        IScenarioThenNoneStateBuilder IScenarioWhenStateBuilder.ThenNone()
        {
            return this;
        }

        IScenarioThrowsStateBuilder IScenarioWhenStateBuilder.ThenException(Exception exception)
        {
            return new ScenarioBuilder(_givens, _when, _thens, exception, _thrownIsAcceptable);
        }

        IScenarioThrowsStateBuilder IScenarioWhenStateBuilder.ThenException(Func<Exception, bool> exceptionIsAcceptable)
        {
            return new ScenarioBuilder(_givens, _when, _thens, _throws, exceptionIsAcceptable);
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

            return new ScenarioBuilder(_givens, command, _thens, _throws, _thrownIsAcceptable);
        }
    }
}
