namespace RoadRegistry.Tests.Framework.Reactions;

using RoadRegistry.BackOffice.Framework;

public static class ReactionScenarioExtensions
{
    public static IReactionScenarioGivenStateBuilder Given(this ReactionScenarioBuilder builder, StreamName stream, params object[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Given(events.Select(@event => new RecordedEvent(stream, @event)));
    }

    public static IReactionScenarioGivenStateBuilder Given(this IReactionScenarioGivenStateBuilder builder, StreamName stream, params object[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Given(events.Select(@event => new RecordedEvent(stream, @event)));
    }

    public static IReactionScenarioGivenStateBuilder Given(this ReactionScenarioBuilder builder, params RecordedEvent[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Given(events);
    }

    public static IReactionScenarioGivenStateBuilder Given(this IReactionScenarioGivenStateBuilder builder, params RecordedEvent[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Given(events);
    }

    public static IReactionScenarioThenStateBuilder Then(this IReactionScenarioGivenNoneStateBuilder builder, StreamName stream, params object[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Then(events.Select(@event => new RecordedEvent(stream, @event)));
    }

    public static IReactionScenarioThenStateBuilder Then(this IReactionScenarioGivenStateBuilder builder, StreamName stream, params object[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Then(events.Select(@event => new RecordedEvent(stream, @event)));
    }

    public static IReactionScenarioThenStateBuilder Then(this IReactionScenarioThenStateBuilder builder, StreamName stream, params object[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Then(events.Select(@event => new RecordedEvent(stream, @event)));
    }

    public static IReactionScenarioThenStateBuilder Then(this IReactionScenarioGivenNoneStateBuilder builder, params RecordedEvent[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Then(events);
    }

    public static IReactionScenarioThenStateBuilder Then(this IReactionScenarioGivenStateBuilder builder, params RecordedEvent[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Then(events);
    }

    public static IReactionScenarioThenStateBuilder Then(this IReactionScenarioThenStateBuilder builder, params RecordedEvent[] events)
    {
        if (events == null)
        {
            throw new ArgumentNullException(nameof(events));
        }

        return builder.Then(events);
    }
}
