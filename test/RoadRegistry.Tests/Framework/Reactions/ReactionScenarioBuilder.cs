namespace RoadRegistry.Tests.Framework.Reactions;

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
        _givens = Array.Empty<RecordedEvent>();
        _thens = Array.Empty<RecordedEvent>();
    }

    private ReactionScenarioBuilder(RecordedEvent[] givens, RecordedEvent[] thens)
    {
        _givens = givens;
        _thens = thens;
    }

    IReactionScenarioThenStateBuilder IReactionScenarioGivenNoneStateBuilder.Then(IEnumerable<RecordedEvent> events)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));
        return new ReactionScenarioBuilder(_givens, _thens.Concat(events).ToArray());
    }

    IReactionScenarioThenNoneStateBuilder IReactionScenarioGivenNoneStateBuilder.ThenNone()
    {
        return new ReactionScenarioBuilder(_givens, Array.Empty<RecordedEvent>());
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

    IReactionScenarioThenNoneStateBuilder IReactionScenarioGivenStateBuilder.ThenNone()
    {
        return new ReactionScenarioBuilder(_givens, Array.Empty<RecordedEvent>());
    }

    IReactionScenarioThenStateBuilder IReactionScenarioThenStateBuilder.Then(IEnumerable<RecordedEvent> events)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));
        return new ReactionScenarioBuilder(_givens, _thens.Concat(events).ToArray());
    }

    ReactionScenario IReactionScenarioBuilder.Build()
    {
        return new ReactionScenario(_givens, _thens);
    }

    public IReactionScenarioGivenNoneStateBuilder GivenNone()
    {
        return new ReactionScenarioBuilder(Array.Empty<RecordedEvent>(), _thens);
    }
}
