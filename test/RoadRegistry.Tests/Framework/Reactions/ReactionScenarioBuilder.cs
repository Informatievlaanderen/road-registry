namespace RoadRegistry.Framework.Reactions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ReactionScenarioBuilder :
        IReactionScenarioGivenStateBuilder,
        IReactionScenarioGivenNoneStateBuilder,
        IReactionScenarioThenStateBuilder,
        IReactionScenarioThenNoneStateBuilder
    {
        private readonly RecordedEvent[] _givens;
        private readonly RecordedEvent[] _thens;

        public ReactionScenarioBuilder()
        {
            _givens = new RecordedEvent[0];
            _thens = new RecordedEvent[0];
        }

        private ReactionScenarioBuilder(RecordedEvent[] givens, RecordedEvent[] thens)
        {
            _givens = givens;
            _thens = thens;
        }

        public IReactionScenarioGivenNoneStateBuilder GivenNone()
        {
            return new ReactionScenarioBuilder(new RecordedEvent[0], _thens);
        }

        public IReactionScenarioGivenStateBuilder Given(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ReactionScenarioBuilder(_givens.Concat(events).ToArray(), _thens);
        }

        IReactionScenarioThenStateBuilder IReactionScenarioGivenStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ReactionScenarioBuilder(_givens, _thens.Concat(events).ToArray());
        }

        IReactionScenarioThenStateBuilder IReactionScenarioGivenNoneStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ReactionScenarioBuilder(_givens, _thens.Concat(events).ToArray());
        }

        IReactionScenarioThenStateBuilder IReactionScenarioThenStateBuilder.Then(IEnumerable<RecordedEvent> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            return new ReactionScenarioBuilder(_givens, _thens.Concat(events).ToArray());
        }

        IReactionScenarioThenNoneStateBuilder IReactionScenarioGivenNoneStateBuilder.ThenNone()
        {
            return new ReactionScenarioBuilder(_givens, new RecordedEvent[0]);
        }

        IReactionScenarioThenNoneStateBuilder IReactionScenarioGivenStateBuilder.ThenNone()
        {
            return new ReactionScenarioBuilder(_givens, new RecordedEvent[0]);
        }

        ReactionScenario IReactionScenarioBuilder.Build()
        {
            return new ReactionScenario(_givens, _thens);
        }
    }
}
